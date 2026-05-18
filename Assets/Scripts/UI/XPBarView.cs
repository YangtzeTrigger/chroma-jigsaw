using System.Collections;
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
            StopAllCoroutines();
            StartCoroutine(AnimateFill(t));
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Refresh();
        }

        public void Hide() => gameObject.SetActive(false);

        private IEnumerator AnimateFill(float target)
        {
            // DOTween: _fill.DOFillAmount(target, 0.5f).SetEase(Ease.OutCubic);
            float elapsed = 0f;
            float start   = _fill.fillAmount;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                _fill.fillAmount = Mathf.Lerp(start, target, elapsed / 0.5f);
                yield return null;
            }
            _fill.fillAmount = target;
        }
    }
}
