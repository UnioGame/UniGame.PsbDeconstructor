namespace UniGame.Tools.PsbDeconstructor
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Utils.Runtime;

    [Serializable]
    public sealed class BuiltInPsbUnPacker : PsbUnPacker
    {
        public override void UnPack(GameObject psbInstance, string spritesFolder, bool createWrappers)
        {
            var children = psbInstance.GetComponentsInChildren<Transform>().ToList();
            children.Remove(psbInstance.transform);
            foreach (var child in children)
            {
                var spriteRenderer = child.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    var spriteName = spriteRenderer.sprite.name;
                    spriteRenderer.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Path.Combine(spritesFolder, spriteName + SpriteExtension));
                    if (!createWrappers) 
                        continue;
                    
                    var wrapper = new GameObject(WrapperPrefix + (int)(-child.position.z * 1000), typeof(SpriteRenderer), typeof(SpriteRendererParent));
                    wrapper.transform.SetParent(child.parent);
                    child.SetParent(wrapper.transform);
                }
                else
                {
                    UnPack(child.gameObject, Path.Combine(spritesFolder, child.name), createWrappers);
                }
            }
        }
    }
}