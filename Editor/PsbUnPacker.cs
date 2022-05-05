namespace UniGame.Tools.PsbDeconstructor
{
    using System;
    using UnityEngine;

    [Serializable]
    public abstract class PsbUnPacker
    {
        protected const string SpriteExtension = ".png";
        
        protected const string WrapperPrefix = "loc_";
        
        public abstract void UnPack(GameObject psbInstance, string spritesFolder, bool createWrappers);
    }
}