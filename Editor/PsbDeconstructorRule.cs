namespace UniGame.Tools.PsbDeconstructor
{
    using System;
    using UniModules.UniGame.CoreModules.UniGame.GraphicsTools.Editor;

#if ODIN_INSPECTOR
    using Sirenix.Utilities.Editor;
#endif
    
    [Serializable]
    public class PsbDeconstructorRule
    {
        public string PathToPsb                     = string.Empty;
        public string TargetPath                    = string.Empty;
        public bool UseCustomSpriteSettings         = false;

#if ODIN_INSPECTOR
        [Title("Importer Settings"), HideLabel, ShowIf("UseCustomSpriteSettings")]
#endif
        public TextureImporterSettingsAdapter ImportSettings = new TextureImporterSettingsAdapter();
    }
}