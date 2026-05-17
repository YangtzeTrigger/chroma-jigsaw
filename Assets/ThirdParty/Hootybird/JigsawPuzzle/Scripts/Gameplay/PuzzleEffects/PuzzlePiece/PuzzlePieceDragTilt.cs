using HootyBird.JigsawPuzzleEngine.Tools;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    [RequireComponent(typeof(PuzzlePieceInteraction))]
    [RequireComponent(typeof(PuzzlePiece))]
    public class PuzzlePieceDragTilt : MonoBehaviour
    {
        [SerializeField]
        private float maxTiltAngle = 20f;
        [SerializeField]
        [Tooltip("In screen percent value. Max tilt angle is applied when piece is moving at tilt speed.")]
        [Range(.002f, .01f)]
        private float tiltSpeed = .005f;

        private PuzzlePieceInteraction interaction;
        private PuzzlePiece puzzlePiece;
        private Quaternion targetRotation;

        private float maxTiltSpeed;
        private bool defaultState;

        private void Awake()
        {
            interaction = GetComponent<PuzzlePieceInteraction>();
            puzzlePiece = GetComponent<PuzzlePiece>();

            if (!interaction || !puzzlePiece)
            {
                enabled = false;
            }

            defaultState = enabled;

            // Only subscribe to events if enabled.
            if (!enabled)
            {
                return;
            }

            maxTiltSpeed = Mathf.Max(Screen.height, Screen.width) * tiltSpeed;
            interaction.OnPieceDrag += OnPieceDrag;

            puzzlePiece.OnSnappedToPuzzleBoard += OnSnappedToBoard;
            puzzlePiece.OnSnappedToPuzzleCluster += OnSnappedToCluster;
            // PuzzlePieces are reused when playing different puzzles. Listen to piece reset event to restore to prefab value.
            puzzlePiece.OnPuzzlePieceReset += OnPuzzlePieceReset;
        }

        /// <summary>
        /// To make "enable" toggle visible.
        /// </summary>
        private void OnEnable() { }

        private void FixedUpdate()
        {
            // Only continue target rotation smoothing if there is angle difference.
            if (targetRotation == Quaternion.identity && transform.localRotation == Quaternion.identity)
            {
                return;
            }

            // If angle difference is less than 1 degree, make it zero.
            if (Quaternion.Angle(targetRotation, transform.localRotation) < 1f)
            {
                transform.localRotation = targetRotation = Quaternion.identity;
                return;
            }

            // Smoothly follow target rotation.
            transform.localRotation = Quaternion.Lerp(
                transform.localRotation, 
                targetRotation, 
                Settings.PuzzleSettings.PuzzlePieceDragTilt_TargetFolloSpeed);
            // And slowly lerp target rotation to zero.
            targetRotation = Quaternion.Lerp(
                Quaternion.identity, 
                targetRotation, 
                Settings.PuzzleSettings.PuzzlePieceDragTilt_TargetToZeroSpeed);
        }

        private void OnPieceDrag(PointerEventData eventData, PuzzlePiece piece)
        {
            // Calculate new target rotation based on pointer event position delta.
            targetRotation = Quaternion.Euler(
                Mathf.Clamp(eventData.delta.y / maxTiltSpeed, -1f, 1f) * maxTiltAngle, 
                Mathf.Clamp(-eventData.delta.x / maxTiltSpeed, -1f, 1f) * maxTiltAngle,
                0f);
        }

        private void OnSnappedToCluster(PuzzlePiece piece, PuzzlePieceEventOrigin eventOrigin)
        {
            DisableAndRestore();
        }

        private void OnSnappedToBoard(PuzzlePiece piece, PuzzlePieceEventOrigin eventOrigin)
        {
            DisableAndRestore();
        }

        private void DisableAndRestore()
        {
            // Restore rotation.
            targetRotation = transform.localRotation = Quaternion.identity;
            // Disable component.
            enabled = false;
        }

        private void OnPuzzlePieceReset(PuzzlePiece piece)
        {
            enabled = defaultState;
        }
    }
}
