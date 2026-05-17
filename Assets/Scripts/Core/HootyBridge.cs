using System;
using System.Collections.Generic;
using UnityEngine;
using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;

namespace ChromaJigsaw.Core
{
    public class HootyBridge : MonoBehaviour
    {
        private static readonly Dictionary<int, int> _xpByPieceCount = new()
        {
            { 12, 10 }, { 24, 25 }, { 48, 60 }, { 96, 150 }
        };

        // rows × cols that yield the target piece count
        private static readonly Dictionary<int, (int rows, int cols)> _gridByPieceCount = new()
        {
            { 12, (3, 4) }, { 24, (4, 6) }, { 48, (6, 8) }, { 96, (8, 12) }
        };

        [SerializeField] private Puzzle _puzzlePrefab;
        [SerializeField] private float  _puzzlePieceSize = 100f;

        private static HootyBridge _instance;
        public static HootyBridge Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<HootyBridge>();
                return _instance;
            }
        }

        private Puzzle _activePuzzle;
        private string _activePuzzleId;
        private int    _piecesOnBoard;

        // Raised for every player-placed snap — wire AudioManager to this.
        public event Action OnPieceSnapped;
        // Raised when puzzle is complete; int = XP awarded.
        public event Action<int> OnPuzzleComplete;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // All Hootybird API calls go through this class — never call Hootybird directly from game logic.

        public void LoadPuzzle(Texture2D image, int pieceCount, string puzzleId)
        {
            if (!_gridByPieceCount.TryGetValue(pieceCount, out var grid))
            {
                Debug.LogError($"[HootyBridge] Unsupported piece count {pieceCount}. Valid: 12, 24, 48, 96.");
                return;
            }

            ClearActivePuzzle();

            _activePuzzleId = puzzleId;
            _piecesOnBoard  = 0;

            var settings = new PuzzleSettings
            {
                id      = puzzleId,
                rows    = grid.rows,
                columns = grid.cols,
            };

            int seed = Math.Abs(puzzleId.GetHashCode());
            PuzzleData puzzleData = PuzzleFactory.FromPuzzleSettings(settings, seed, puzzleId);

            SavedGameData savedGame = SaveGameService.HaveSaveFile(puzzleId)
                ? SaveGameService.LoadSavedGameData(puzzleId)
                : null;

            _activePuzzle = Instantiate(_puzzlePrefab);

            // Subscribe per-piece as each is created so we capture SaveFile snaps during init
            // (needed for accurate _piecesOnBoard when resuming a saved game).
            _activePuzzle.OnPuzzlePieceCreated += piece =>
                piece.OnSnappedToPuzzleBoard += HandlePieceSnapped;

            _activePuzzle.Initialize(puzzleData, image, savedGame, _puzzlePieceSize);
        }

        private void HandlePieceSnapped(PuzzlePiece piece, PuzzlePieceEventOrigin origin)
        {
            _piecesOnBoard++;

            if (origin != PuzzlePieceEventOrigin.Player) return;

            OnPieceSnapped?.Invoke();
            SaveGameService.SaveGame(_activePuzzle);

            if (_piecesOnBoard >= _activePuzzle.PuzzlePieces.Count)
                CompletePuzzle();
        }

        private void CompletePuzzle()
        {
            _xpByPieceCount.TryGetValue(_activePuzzle.PuzzlePieces.Count, out int xp);
            SaveGameService.DeleteSavedGameData(_activePuzzleId);
            OnPuzzleComplete?.Invoke(xp);
        }

        public void SaveProgress()
        {
            if (_activePuzzle != null)
                SaveGameService.SaveGame(_activePuzzle);
        }

        private void ClearActivePuzzle()
        {
            if (_activePuzzle == null) return;
            foreach (var piece in _activePuzzle.PuzzlePieces)
                piece.OnSnappedToPuzzleBoard -= HandlePieceSnapped;
            _activePuzzle.Clear();
            Destroy(_activePuzzle.gameObject);
            _activePuzzle = null;
        }

        private void OnDestroy() => ClearActivePuzzle();
    }
}
