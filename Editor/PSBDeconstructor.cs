namespace UniGame.Tools.PsbDeconstructor
{
    using UniModules.Editor;
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Linq;
    using UniModules.UniGame.AtlasGenerator.Editor;
    using UniModules.UniGame.CoreModules.UniGame.GraphicsTools.Editor;
    using UniModules.UniGame.GraphicsTools.Editor.SpriteAtlas;

    public static class PsbDeconstructor
    {
        private const string PsbExtension    = ".psb";
        private const string PrefabExtension = ".prefab";
        
        private const string DefaultSettings    = "DefaultTexturePlatform";
        private const string StandaloneSettings = "Standalone";
        private const string AndroidSettings    = "Android";
        
        private const string SpriteExtension = ".png";

        [MenuItem("Assets/Unpack PSB")]
        private static void UnpackPsb()
        {
            var settings = GetSettings();
            if (settings == null)
            {
                return;
            }

            var psbList = new List<EditorResource>();
            foreach (var obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.DeepAssets))
            {
                if (obj.GetType() != typeof(GameObject))
                {
                    continue;
                }
                string path = AssetDatabase.GetAssetPath(obj);
                if (Path.GetExtension(path).Equals(PsbExtension, StringComparison.InvariantCultureIgnoreCase))
                {
                    psbList.Add(obj.ToEditorResource());
                }
            }

            var textureFactory = new TextureFactory();
            var spriteFactory = new SpriteAssetFactory(new SpriteAtlasSettings());

            foreach (var psb in psbList)
            {
                if (CheckPsbPath(settings, psb.AssetPath, out var rule, out var targetPath))
                {
                    UnpackObject(settings, rule, psb.asset as GameObject, targetPath, textureFactory, spriteFactory);
                    MovePsb(settings, psb.asset as GameObject);
                }
            }
        }

        private static PsbDeconstructorSettings GetSettings()
        {
            var settings = PsbDeconstructorSettings.Settings;
            if (settings == null)
            {
                Debug.LogError($"[{nameof(PsbDeconstructor)}] Can't load settings");
            }
            return settings;
        }

        private static bool CheckPsbPath(PsbDeconstructorSettings settings, string psbPath, out PsbDeconstructorRule rule, out string targetPath)
        {
            targetPath = string.Empty;
            rule = null;

            foreach (var ruleSetting in settings.RuleSettings)
            {
                var regex = new Regex(ruleSetting.PathToPsb);
                var match = regex.Match(psbPath);
                if (match.Success)
                {
                    rule = ruleSetting;
                    targetPath = regex.Replace(psbPath, ruleSetting.TargetPath);
                    return true;
                }
            }

            return false;
        }

        private static void UnpackObject(PsbDeconstructorSettings settings, PsbDeconstructorRule rule, GameObject asset, string unpackPath, TextureFactory textureFactory, SpriteAssetFactory spriteFactory)
        {
            EditorFileUtils.CreateDirectories(unpackPath);

            var isMainAsset = AssetDatabase.IsMainAsset(asset);
            if (isMainAsset)
            {
                unpackPath = Path.Combine(unpackPath, asset.name);
            }

            foreach (Transform layer in asset.transform)
            {
                if (layer.gameObject.GetComponent<SpriteRenderer>() != null)
                {
                    UnpackSprite(layer, textureFactory, unpackPath, settings, rule, spriteFactory);
                    continue;
                }

                var newFolderPath = Path.Combine(unpackPath, layer.gameObject.name);
                UnpackObject(settings, rule, layer.gameObject, newFolderPath, textureFactory, spriteFactory);
            }

            if (!isMainAsset) 
                return;
            
            var instance = UnityEngine.Object.Instantiate(asset);
            SetSortingOrder(instance);
            settings.DefaultUnPacker.UnPack(instance, unpackPath, settings.ImportSettings.CreateLocatorWrappers);
            
            PrefabUtility.SaveAsPrefabAsset(instance, unpackPath + PrefabExtension);
            UnityEngine.Object.DestroyImmediate(instance);
        }

        private static void UnpackSprite(Transform layer, TextureFactory textureFactory, string unpackPath, 
            PsbDeconstructorSettings settings, PsbDeconstructorRule rule, SpriteAssetFactory spriteFactory)
        {
            var sprite = layer.gameObject.GetComponent<SpriteRenderer>();
            if (!sprite) return;
            
            var uncrunchedTexture = sprite.sprite.texture.Decompress();
            var uncrunchedSprite = Sprite.Create(uncrunchedTexture, sprite.sprite.rect, sprite.sprite.pivot);
            uncrunchedSprite.name = sprite.name;
            var textures = textureFactory.Create(uncrunchedSprite);

            if (textures == null) 
                return;
            
            foreach (var texture in textures)
            {
                var filePath = Path.Combine(unpackPath, texture.name + SpriteExtension);
                filePath = filePath.FixUnityPath();

                var platformSettings = new Dictionary<string, TextureImporterPlatformSettings> { { DefaultSettings, null }, { StandaloneSettings, null }, { AndroidSettings, null } };
                GetImporterSettings(settings, rule, filePath, platformSettings, out var importerSettings);
                var filledPlatformSettings = platformSettings
                    .Where(pair => pair.Value != null)
                    .Select(pair => pair.Value)
                    .ToList();

                var data = new SpriteAssetFactoryInputData
                {
                    Name = texture.name,
                    Path = unpackPath,
                    Reference = texture,
                    ImporterSettings = importerSettings,
                    ImporterPlatformSettings = filledPlatformSettings
                };

                spriteFactory.Create(data);
            }
        }
        
        private static void MovePsb(PsbDeconstructorSettings settings, GameObject asset)
        {
            if (string.IsNullOrEmpty(settings.PathToMovePSB.Trim()))
            {
                return;
            }

            var oldPath = AssetDatabase.GetAssetPath(asset);
            if (oldPath.StartsWith(settings.PathToMovePSB))
            {
                return;
            }
            var newPath = oldPath.Replace(settings.PSBRootDirectory, settings.PathToMovePSB);
            EditorFileUtils.CreateDirectories(newPath);
            AssetDatabase.MoveAsset(oldPath, newPath);
        }

        private static void SetSortingOrder(GameObject asset)
        {
            foreach (Transform child in asset.transform)
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    child.position = new Vector3(child.position.x, child.position.y, -renderer.sortingOrder / 1000f);
                    renderer.sortingOrder = 0;
                } else
                {
                    SetSortingOrder(child.gameObject);
                }
            }
        }

        private static void GetImporterSettings(PsbDeconstructorSettings settings, PsbDeconstructorRule rule, string filePath, 
            Dictionary<string, TextureImporterPlatformSettings> platformSettings, out TextureImporterSettings importerSettings)
        {
            importerSettings = new TextureImporterSettings();

            if (rule.UseCustomSpriteSettings && rule.ImportSettings.ApplicationMode == TextureImporterSettingsAdapter.SettingsApplicationMode.OnlyApplyOnCreation ||
                !rule.UseCustomSpriteSettings && settings.ImportSettings.ApplicationMode == TextureImporterSettingsAdapter.SettingsApplicationMode.OnlyApplyOnCreation)
            {
                if (File.Exists(filePath))
                {
                    var importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                    importer.ReadTextureSettings(importerSettings);
                    platformSettings[DefaultSettings] = importer.GetDefaultPlatformTextureSettings();
                    platformSettings[StandaloneSettings] = importer.GetPlatformTextureSettings(StandaloneSettings);
                    platformSettings[AndroidSettings] = importer.GetPlatformTextureSettings(AndroidSettings);
                    return;
                }
            }

            if (rule.UseCustomSpriteSettings)
            {
                importerSettings = rule.ImportSettings.GetTextureImporterSettings();
                GetImporterPlatformSettings(rule.ImportSettings, platformSettings);

            } else
            {
                importerSettings = settings.ImportSettings.GetTextureImporterSettings();
                GetImporterPlatformSettings(settings.ImportSettings, platformSettings);
            }
        }
        
        private static void GetImporterPlatformSettings(TextureImporterSettingsAdapter settings, Dictionary<string, TextureImporterPlatformSettings> platformSettings)
        {
            platformSettings[DefaultSettings] = settings.GetTextureImporterPlatformSettings();
            platformSettings[StandaloneSettings] = settings.GetTextureImporterPlatformSettings(BuildTargetGroup.Standalone);
            platformSettings[AndroidSettings] = settings.GetTextureImporterPlatformSettings(BuildTargetGroup.Android);
        }
    }
}