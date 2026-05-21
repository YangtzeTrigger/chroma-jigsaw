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

        // Snap radius tiers per piece count: generous (12/24) → tighter (48) → precise (96)
        private static readonly Dictionary<int, (float min, float max, float deviation)> _snapByPieceCount = new()
        {
            { 12, (0.8f, 2.0f, 0.4f) },
            { 24, (0.8f, 2.0f, 0.4f) },
            { 48, (0.8f, 1.5f, 0.3f) },
            { 96, (0.8f, 1.2f, 0.2f) },
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
        private int    _draggingCount;

        public event Action OnPiecePickedUp;   // finger down on a piece
        public event Action OnPiecePlaced;     // finger released (piece set down, may or may not snap)
        public event Action OnPieceSnapped;    // piece locked to board by player
        public event Action<int> OnPuzzleComplete;
        public event Action<Puzzle, PuzzlePanelInteraction> OnPuzzleLoaded;

        public bool IsAnyPieceDragging => _draggingCount > 0;

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
            _draggingCount  = 0;

            if (_snapByPieceCount.TryGetValue(pieceCount, out var snap))
            {
                Settings.PuzzleSettings.PieceSnapPieceMinDistance      = snap.min;
                Settings.PuzzleSettings.PieceSnapPieceMaxDistance      = snap.max;
                Settings.PuzzleSettings.PieceSnapMaxDirectionDeviation = snap.deviation;
            }

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

            _activePuzzle.OnPuzzleInitialized += OnPuzzleInitialized;
            _activePuzzle.Initialize(puzzleData, image, savedGame, _puzzlePieceSize);
        }

        private void OnPuzzleInitialized(bool fromSave)
        {
            _activePuzzle.OnPuzzleInitialized -= OnPuzzleInitialized;

            foreach (var piece in _activePuzzle.PuzzlePieces)
            {
                var interaction = piece.GetComponent<PuzzlePieceInteraction>();
                if (interaction == null) continue;
                interaction.OnPiecePointerDown += (_, _) =>
                {
                    _draggingCount++;
                    OnPiecePickedUp?.Invoke();
                };
                interaction.OnPiecePointerUp += (_, _) =>
                {
                    _draggingCount = Mathf.Max(0, _draggingCount - 1);
                    OnPiecePlaced?.Invoke();
                };
            }

            var panelInteraction = _activePuzzle.GetComponent<PuzzlePanelInteraction>();
            OnPuzzleLoaded?.Invoke(_activePuzzle, panelInteraction);
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
            _draggingCount = 0;
            foreach (var piece in _activePuzzle.PuzzlePieces)
                piece.OnSnappedToPuzzleBoard -= HandlePieceSnapped;
            _activePuzzle.Clear();
            Destroy(_activePuzzle.gameObject);
            _activePuzzle = null;
        }

        private void OnDestroy() => ClearActivePuzzle();
    }
}
