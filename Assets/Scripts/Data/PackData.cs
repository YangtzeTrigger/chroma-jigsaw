using System;

namespace ChromaJigsaw.Data
{
    [Serializable]
    public class ImageEntry
    {
        public string imageId;
        public string displayName;
    }

    [Serializable]
    public class PackDefinition
    {
        public string   packId;
        public string   packName;
        public string   theme;
        public string[] imageIds;                    // always 12
        public bool     isOwned;
        public bool     completionBadge;
        public bool     grandMasterpieceUnlocked;
    }

    [Serializable]
    public class PackCatalogueData
    {
        public PackDefinition[] packs;
        public int              catalogueVersion;
    }
}
