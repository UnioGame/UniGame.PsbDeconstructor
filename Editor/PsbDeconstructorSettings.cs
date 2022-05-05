namespace UniGame.Tools.PsbDeconstructor
{
    using UniModules.UniGame.Core.Editor.EditorProcessors;
    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UniModules.UniGame.CoreModules.UniGame.GraphicsTools.Editor;

    [GeneratedAssetInfo("Assets/UniGame.Generated/PsbDeconstructor/Editor")]
    public class PsbDeconstructorSettings : GeneratedAsset<PsbDeconstructorSettings>
    {
        #region static data
        
        public static PsbDeconstructorSettings Settings => Asset;

        #endregion

        [SerializeField]
        [FolderPath, TitleGroup("Paths")]
        [Tooltip("If PSB file is moved after unpacking, folders contained inside the root directory will be added to the new path.")]
        public string PSBRootDirectory = "Assets/GameContent/Art";

        [SerializeField]
        [FolderPath, TitleGroup("Paths")]
        [Tooltip("Path to directory where PSB file will be moved after unpacking. If empty, PSB file won't be moved.")]
        public string PathToMovePSB;

        [SerializeReference]
        [TitleGroup("Additional Settings")]
        private PsbUnPacker _defaultUnPacker;

        [SerializeField]
        [TitleGroup("Rules"), LabelText("Rules")]
        public List<PsbDeconstructorRule> RuleSettings = new List<PsbDeconstructorRule>();

        [SerializeField]
        [Title("Default Importer Settings"), HideLabel]
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