using HootyBird.JigsawPuzzleEngine.Tween;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    [RequireComponent(typeof(PuzzlePieceInteraction))]
    [RequireComponent(typeof(PuzzlePiece))]
    public class PuzzlePieceShadow : MonoBehaviour
    {
        [SerializeField]
        private float shadowOffset = 10f;
        [SerializeField]
        private Color shadowColor = Color.black;
        [SerializeField]
        [Tooltip("What's the expected max tilt anlge for the object.")]
        private float referenceMaxTiltAngle = 60f;

        private PuzzlePiece puzzlePiece;
        private Shadow shadowComponent;
        private ColorTween colorTween;

        private bool defaultState;

        private void Awake()
        {
            puzzlePiece = GetComponent<PuzzlePiece>();

            if (!puzzlePiece) 
            { 
                enabled = false;
            }

            defaultState = enabled;

            // Only subscribe to events if enabled.
            if (!enabled)
            {
                return;
            }

            // Init shadow component.
            shadowComponent = puzzlePiece.RenderTarget.gameObject.AddComponent<Shadow>();
            shadowComponent.effectColor = Color.clear;

            // Init shadow tween component.
            colorTween = gameObject.AddComponent<ColorTween>();
            colorTween.from = Color.clear;
            colorTween.to = shadowColor;
            colorTween.OnColorUpdate += OnColorTweenValueUpdated;

            puzzlePiece.OnSnappedToPuzzleBoard += OnSnappedToBoard;
            puzzlePiece.OnSnappedToPuzzleCluster += OnSnappedToCluster;
            // PuzzlePieces are reused when playing different puzzles. Listen to piece reset event to restore to prefab value.
            puzzlePiece.OnPuzzlePieceReset += OnPuzzlePieceReset;
        }

        /// <summary>
        /// To make "enable" toggle visible.
        /// </summary>
        private void OnEnable() { }

        private void Update()
        {
            if (puzzlePiece.Cluster != null)
            {
                return;
            }

            // Modify shadow effectDistance value using object rotation value.
            if (transform.localRotation == Quaternion.identity)
            {
                if (shadowComponent.effectDistance != Vector2.zero)
                {
                    shadowComponent.effectDistance = Vector2.zero;
                    shadowComponent.effectColor = Color.clear;
                }
            }
            else
            {
                Vector2 pieceRotation = puzzlePiece.transform.eulerAngles;
                // Fix rotation values.
                pieceRotation.x %= 360f;
                if (pieceRotation.x > 180f)
                {
                    pieceRotation.x -= 360f;
                }
                pieceRotation.y %= 360f;
                if (pieceRotation.y > 180f)
                {
                    pieceRotation.y -= 360f;
                }

                // Calculate shadow distance from tilt.
                Vector2 nomalizedRotation = pieceRotation / referenceMaxTiltAngle;
                Vector2 shadowAxis = 
                    Quaternion.AngleAxis(puzzlePiece.RotationParent.eulerAngles.z, Vector3.forward) * 
                    new Vector2(nomalizedRotation.y, -nomalizedRotation.x);
                colorTween.AtProgress(Mathf.Clamp01(nomalizedRotation.magnitude));

                // Flip axis for LEFT/RIGHT direction.
                if (puzzlePiece.Direction == PuzzlePieceDirection.Right || puzzlePiece.Direction == PuzzlePieceDirection.Left)
                {
                    shadowAxis = -shadowAxis;
                }
                shadowComponent.effectDistance = shadowAxis * shadowOffset;
            }
        }

        private void OnColorTweenValueUpdated(Color value)
        {
            shadowComponent.effectColor = value;
        }

        private void OnSnappedToBoard(PuzzlePiece puzzlePiece, PuzzlePieceEventOrigin eventOrigin)
        {
            shadowComponent.effectColor = Color.clear;
            enabled = false;
        }

        private void OnSnappedToCluster(PuzzlePiece puzzlePiece, PuzzlePieceEventOrigin eventOrigin)
        {
            shadowComponent.effectColor = Color.clear;
            enabled = false;
        }

        private void OnPuzzlePieceReset(PuzzlePiece piece)
        {
            enabled = defaultState;
            colorTween.playbackDirection = PlaybackDirection.FORWARD;

            shadowComponent.effectColor = Color.clear;
        }
    }
}
