using UnityEngine;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.Audio
{
    // Bridges HootyBridge events (Core) to AudioManager SFX calls (Audio).
    // Place one instance in the Game scene alongside HootyBridge.
    public class PuzzleAudioController : MonoBehaviour
    {
        private void OnEnable()
        {
            if (HootyBridge.Instance == null) return;
            HootyBridge.Instance.OnPiecePickedUp  += OnPickedUp;
            HootyBridge.Instance.OnPiecePlaced    += OnPlaced;
            HootyBridge.Instance.OnPieceSnapped   += OnSnapped;
            HootyBridge.Instance.OnPuzzleComplete += OnComplete;
        }

        private void OnDisable()
        {
            if (HootyBridge.Instance == null) return;
            HootyBridge.Instance.OnPiecePickedUp  -= OnPickedUp;
            HootyBridge.Instance.OnPiecePlaced    -= OnPlaced;
            HootyBridge.Instance.OnPieceSnapped   -= OnSnapped;
            HootyBridge.Instance.OnPuzzleComplete -= OnComplete;
        }

        private void OnPickedUp()  => AudioManager.Instance.Play(SFXType.PiecePickup);
        private void OnPlaced()    => AudioManager.Instance.Play(SFXType.PiecePlaced);
        private void OnSnapped()   => AudioManager.Instance.Play(SFXType.PieceSnap);
        private void OnComplete(int _) => AudioManager.Instance.Play(SFXType.PuzzleComplete);
    }
}
