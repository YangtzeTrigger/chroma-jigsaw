using System;
using UnityEngine;

namespace ChromaJigsaw.Core
{
    [CreateAssetMenu(fileName = "DailyManifest", menuName = "Chroma Jigsaw/Daily Manifest")]
    public class DailyManifest : ScriptableObject
    {
        public DailyEntry[] entries; // always 3; index 0 = newest, index 2 = oldest
    }

    [Serializable]
    public class DailyEntry
    {
        public string dailyId;          // "2026-05-19"
        public string title;            // Jason writes — never auto-generate
        public int    pieceCount;       // 12 or 24 only
        public Sprite imageSprite;      // 1080p square
        public string photographyMeta; // Jason writes — never auto-generate
        public string overlayScript;   // Jason writes — never auto-generate
    }
}
