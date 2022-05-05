namespace UniGame.Tools.PsbDeconstructor
{
    using System;
    using Sirenix.OdinInspector;
    using UniModules.UniGame.CoreModules.UniGame.GraphicsTools.Editor;

    [Serializable]
    public class PsbDeconstructorRule
    {
        public string PathToPsb                     = string.Empty;
        public string TargetPath                    = string.Empty;
        public bool UseCustomSpriteSettings         = false;

        [Title("Importer Settings"), HideLabel, ShowIf("UseCustomSpriteSettings")]
        public TextureImporterSettingsAdapter ImportSettings = new TextureImporterSettingsAdapter();
    }
}