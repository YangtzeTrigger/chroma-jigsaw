using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Tools;
using UnityEngine;
using UnityEngine.Video;

namespace HootyBird.JigsawPuzzleEngine.Experimental
{
    public class VideoPuzzleTest : MonoBehaviour
    {
        [SerializeField]
        private Puzzle puzzle;
        [SerializeField]
        private PuzzleSettingsObject settings;
        [SerializeField]
        private RenderTexture puzzleTexture;
        [SerializeField]
        private VideoPlayer player;

        private RectTransform rectTransform;
        private bool initialized = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            player.started += PlayerStarted;
            puzzle.OnPuzzleInitialized += OnPuzzleInitialized;
        }

        private void PlayerStarted(VideoPlayer source)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            puzzle.Initialize(PuzzleFactory.FromPuzzleSettings(settings.PuzzleSettings, 0), puzzleTexture);
        }

        private void OnPuzzleInitialized(bool reinitialized)
        {
            // Update puzzle pieces restriction rects.
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Rect panelRect = new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[2] - corners[0]).y);
            foreach (PuzzlePiece piece in puzzle.PuzzlePieces)
            {
                piece.SetRestrictionWorldRect(panelRect);
            }
        }
    }
}

