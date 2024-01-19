namespace UniGame.Tools.PsbDeconstructor
{
    using UniModules.UniGame.Core.Editor.EditorProcessors;
    using UnityEngine;
    using System.Collections.Generic;
    using UniModules.UniGame.CoreModules.UniGame.GraphicsTools.Editor;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    
    [GeneratedAssetInfo("Assets/UniGame.Generated/PsbDeconstructor/Editor")]
    public class PsbDeconstructorSettings : GeneratedAsset<PsbDeconstructorSettings>
    {
        #region static data
        
        public static PsbDeconstructorSettings Settings => Asset;

        #endregion

        [SerializeField]
#if ODIN_INSPECTOR
        [FolderPath, TitleGroup("Paths")]
#endif        
        [Tooltip("If PSB file is moved after unpacking, folders contained inside the root directory will be added to the new path.")]
        public string PSBRootDirectory = "Assets/GameContent/Art";

        [SerializeField]
#if ODIN_INSPECTOR
        [FolderPath, TitleGroup("Paths")]
#endif
        [Tooltip("Path to directory where PSB file will be moved after unpacking. If empty, PSB file won't be moved.")]
        public string PathToMovePSB;

        [SerializeReference]
#if ODIN_INSPECTOR
        [TitleGroup("Additional Settings")]
#endif
        private PsbUnPacker _defaultUnPacker;

        [SerializeField]
#if ODIN_INSPECTOR
        [TitleGroup("Rules"), LabelText("Rules")]
#endif
        public List<PsbDeconstructorRule> RuleSettings = new List<PsbDeconstructorRule>();

        [SerializeField]
#if ODIN_INSPECTOR
        [Title("Default Importer Settings"), HideLabel]
#endif
        public TextureImporterSettingsAdapter ImportSettings = new TextureImporterSettingsAdapter();

        public PsbUnPacker DefaultUnPacker
        {
            get
            {
                if(_defaultUnPacker == null)
                    _defaultUnPacker = new BuiltInPsbUnPacker();

                return _defaultUnPacker;
            }
        }
    }
}