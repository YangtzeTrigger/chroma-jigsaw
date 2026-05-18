using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChromaJigsaw.UI
{
    public class ArtworkSpotlightView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     _canvasGroup;
        [SerializeField] private RawImage        _artworkImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Button          _closeButton;
        [SerializeField] private Button          _replayButton;
        [SerializeField] private Button          _shareButton;
        [SerializeField] private Button          _wallpaperButton;

        private void Awake()
        {
            _canvasGroup.alpha          = 0f;
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);

            _closeButton.onClick.AddListener(Close);
            _replayButton.onClick.AddListener(HandleReplay);
            _shareButton.onClick.AddListener(HandleShare);
            _wallpaperButton.onClick.AddListener(HandleWallpaper);
        }

        public void Open(string packId, string imageId)
        {
            gameObject.SetActive(true);
            _titleText.text = imageId.ToUpper();
            // TODO: load Texture2D for imageId via asset pipeline (B-06+)
            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }

        public void Close()
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeIn()
        {
            // DOTween: _canvasGroup.DOFade(1f, 0.3f);
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable   = true;
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 0.3f);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            // DOTween: _canvasGroup.DOFade(0f, 0.25f).OnComplete(() => gameObject.SetActive(false));
            _canvasGroup.interactable = false;
            float elapsed = 0f;
            float start   = _canvasGroup.alpha;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, 0f, elapsed / 0.25f);
                yield return null;
            }
            _canvasGroup.alpha          = 0f;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        private void HandleReplay()
        {
            // Stub — puzzle replay animation wired in a later stage
#if UNITY_EDITOR
            Debug.Log("[Spotlight] Replay pressed");
#endif
        }

        private void HandleShare()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var intent      = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intent.Call<AndroidJavaObject>("setType", "text/plain");
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),
                "Check out my puzzle on Chroma Jigsaw!");
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var chooser     = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share via");
            activity.Call("startActivity", chooser);
#endif
        }

        private void HandleWallpaper()
        {
            // Stub — WallpaperManager API wired in B-08
#if UNITY_EDITOR
            Debug.Log("[Spotlight] Set Wallpaper pressed");
#endif
        }
    }
}
