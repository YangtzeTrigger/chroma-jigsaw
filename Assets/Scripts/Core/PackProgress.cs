using System;
using System.Collections.Generic;

namespace ChromaJigsaw.Core
{
    [Serializable]
    public class PackProgress
    {
        public string       packId;
        public bool         isOwned;
        public List<string> completedImageIds        = new List<string>();
        public bool         grandMasterpieceUnlocked;
    }
}
