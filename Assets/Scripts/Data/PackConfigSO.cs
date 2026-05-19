using UnityEngine;

namespace ChromaJigsaw.Data
{
    public enum PackUnlockType { Free, XP, IAP, ZenPass }

    [CreateAssetMenu(fileName = "PackConfig", menuName = "Chroma Jigsaw/Pack Config")]
    public class PackConfigSO : ScriptableObject
    {
        public string        packId;
        public string        packName;
        [TextArea]
        public string        description;
        public Sprite        thumbnailSprite;
        public string[]      imageIds;
        public int           pieceCount    = 24;
        public PackUnlockType unlockType   = PackUnlockType.Free;
        public int           xpRequired;
        public string        iapProductId;
    }
}
