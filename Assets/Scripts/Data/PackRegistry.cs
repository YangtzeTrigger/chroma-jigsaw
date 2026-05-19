using UnityEngine;

namespace ChromaJigsaw.Data
{
    [CreateAssetMenu(fileName = "PackRegistry", menuName = "Chroma Jigsaw/Pack Registry")]
    public class PackRegistry : ScriptableObject
    {
        public PackConfigSO[] packs;
    }
}
