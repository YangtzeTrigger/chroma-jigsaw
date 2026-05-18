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
            HootyBridge.Instance.OnPieceSnapped   += HandlePieceSnapped;
            HootyBridge.Instance.OnPuzzleComplete += HandlePuzzleComplete;
        }

        private void OnDisable()
        {
            if (HootyBridge.Instance == null) return;
            HootyBridge.Instance.OnPieceSnapped   -= HandlePieceSnapped;
            HootyBridge.Instance.OnPuzzleComplete -= HandlePuzzleComplete;
        }

        private void HandlePieceSnapped() =>
            AudioManager.Instance.PlaySFX(SFXType.PieceSnap);

        private void HandlePuzzleComplete(int xp) =>
            AudioManager.Instance.PlaySFX(SFXType.PuzzleComplete);
    }
}
