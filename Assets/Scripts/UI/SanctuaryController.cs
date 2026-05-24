using ChromaJigsaw.Data;
using UnityEngine;

namespace ChromaJigsaw.UI
{
    public class SanctuaryController : MonoBehaviour
    {
        [SerializeField] private DailyCardView        _dailyCardView;
        [SerializeField] private DailyMasterpieceView _masterpieceView;

        private void OnEnable()
        {
            if (_dailyCardView != null)
                _dailyCardView.Refresh(OpenDailyMasterpiece);
            AnalyticsManager.Instance.LogScreenView("sanctuary_landing");
        }

        private void OpenDailyMasterpiece()
        {
            if (_masterpieceView == null) return;
            _masterpieceView.Open(0);
            AnalyticsManager.Instance.LogScreenView("daily_masterpiece");
        }
    }
}
