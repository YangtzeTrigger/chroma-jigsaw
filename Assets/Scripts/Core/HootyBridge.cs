using System;
using UnityEngine;

namespace ChromaJigsaw.Core
{
    public class HootyBridge : MonoBehaviour
    {
        private static HootyBridge _instance;
        public static HootyBridge Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<HootyBridge>();
                if (_instance == null) { var go = new GameObject("[HootyBridge]"); _instance = go.AddComponent<HootyBridge>(); }
                return _instance;
            }
        }

        public event Action<int> OnPieceSnapped;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // All Hootybird API calls go through this class — never call Hootybird directly from game logic.

        public void LoadPuzzle(Texture2D image, int pieceCount)
        {
            // HootyBird.JigsawManager.Instance.LoadPuzzle(image, pieceCount);
        }

        internal void NotifyPieceSnapped(int pieceId) => OnPieceSnapped?.Invoke(pieceId);
    }
}
