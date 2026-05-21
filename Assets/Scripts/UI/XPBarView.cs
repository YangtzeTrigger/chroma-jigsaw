using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.UI
{
    public class XPBarView : MonoBehaviour
    {
        [SerializeField] private Image           _fill;
        [SerializeField] private TextMeshProUGUI _xpLabel;
        [SerializeField] private int             _xpPerLevel = 10000;

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            int xp      = SaveManager.Instance.Data.totalXP;
            int current = xp % _xpPerLevel;
            float t     = (float)current / _xpPerLevel;
            _xpLabel.text = $"XP  {current:N0} / {_xpPerLevel:N0}";
            _fill.DOKill();
            _fill.DOFillAmount(t, 0.8f).SetEase(Ease.OutCubic);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
