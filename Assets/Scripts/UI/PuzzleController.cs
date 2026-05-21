using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;
using UnityEngine;

namespace ChromaJigsaw.UI
{
    // Attach to a GameObject in the Game scene.
    // Routes HootyBridge events to AudioManager SFX.
    public class PuzzleController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (HootyBridge.Instance == null) return;
            HootyBridge.Instance.OnPiecePickedUp  += HandlePiecePickedUp;
            HootyBridge.Instance.OnPiecePlaced    += HandlePiecePlaced;
            HootyBridge.Instance.OnPieceSnapped   += HandlePieceSnapped;
            HootyBridge.Instance.OnPuzzleComplete += HandlePuzzleComplete;
        }

        private void OnDisable()
        {
            if (HootyBridge.Instance == null) return;
            HootyBridge.Instance.OnPiecePickedUp  -= HandlePiecePickedUp;
            HootyBridge.Instance.OnPiecePlaced    -= HandlePiecePlaced;
            HootyBridge.Instance.OnPieceSnapped   -= HandlePieceSnapped;
            HootyBridge.Instance.OnPuzzleComplete -= HandlePuzzleComplete;
        }

        private void HandlePiecePickedUp()      => AudioManager.Instance.Play(SFXType.PiecePickup);
        private void HandlePiecePlaced()        => AudioManager.Instance.Play(SFXType.PiecePlaced);
        private void HandlePieceSnapped()       => AudioManager.Instance.Play(SFXType.PieceSnap);
        private void HandlePuzzleComplete(int _) => AudioManager.Instance.Play(SFXType.PuzzleComplete);
    }
}
