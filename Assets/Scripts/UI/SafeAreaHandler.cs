using UnityEngine;

namespace ChromaJigsaw.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHandler : MonoBehaviour
    {
        private RectTransform _rt;
        private Rect _lastSafeArea;

        private void Awake()
        {
            _rt = GetComponent<RectTransform>();
            Apply();
        }

        private void Apply()
        {
            var safeArea = Screen.safeArea;
            if (safeArea == _lastSafeArea) return;
            _lastSafeArea = safeArea;

            var screen = new Vector2(Screen.width, Screen.height);
            _rt.anchorMin = new Vector2(safeArea.xMin / screen.x, safeArea.yMin / screen.y);
            _rt.anchorMax = new Vector2(safeArea.xMax / screen.x, safeArea.yMax / screen.y);
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
