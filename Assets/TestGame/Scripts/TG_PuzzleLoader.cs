using UnityEngine;
using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;

// Minimal Hootybird integration — no ChromaJigsaw wrappers.
// Wire _puzzle to the Puzzle prefab instance in the scene.
// Wire _testTexture to any Texture2D (e.g. cat01_2k).
public class TG_PuzzleLoader : MonoBehaviour
{
    [SerializeField] private Puzzle    _puzzle;
    [SerializeField] private Texture2D _testTexture;

    [Header("Puzzle Settings")]
    [SerializeField] private int _rows    = 4;
    [SerializeField] private int _columns = 6;
    [SerializeField] private float _pieceSize = 100f;

    private void Start()
    {
        if (_puzzle == null)      { Debug.LogError("[TG] _puzzle is not assigned."); return; }
        if (_testTexture == null) { Debug.LogError("[TG] _testTexture is not assigned."); return; }

        var settings = new PuzzleSettings
        {
            id      = "tg_test_001",
            rows    = _rows,
            columns = _columns,
        };

        int seed = Mathf.Abs("tg_test_001".GetHashCode());
        PuzzleData data = PuzzleFactory.FromPuzzleSettings(settings, seed, "tg_test_001");

        _puzzle.OnPuzzleInitialized += fromSave =>
            Debug.Log($"[TG] Puzzle initialized — fromSave={fromSave} pieces={_puzzle.PuzzlePieces?.Count}");

        _puzzle.Initialize(data, _testTexture, null, _pieceSize);
        Debug.Log("[TG] Initialize called.");
    }
}
