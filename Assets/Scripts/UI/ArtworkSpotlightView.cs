using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Data;

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

        private string _packId;
        private string _imageId;

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
            _packId = packId;
            _imageId = imageId;
            gameObject.SetActive(true);
            _titleText.text = imageId.ToUpper();
            // TODO: load Texture2D for imageId via asset pipeline (B-06+)
            _canvasGroup.DOKill();
            transform.DOKill();
            _canvasGroup.alpha          = 1f;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable   = true;
            transform.localScale        = Vector3.one * 0.92f;
            transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        public void Close()
        {
            _canvasGroup.DOKill();
            transform.DOKill();
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
            transform.DOScale(Vector3.one * 0.92f, 0.25f)
                     .SetEase(Ease.InCubic)
                     .OnComplete(() =>
                     {
                         _canvasGroup.alpha = 0f;
                         gameObject.SetActive(false);
                     });
        }

        private void HandleReplay()
        {
            AnalyticsManager.Instance.LogEvent("artwork_replayed", new Dictionary<string, object>
            {
                { "pack_id",  _packId  },
                { "image_id", _imageId }
            });
            // Stub — puzzle replay animation wired in a later stage
#if UNITY_EDITOR
            Debug.Log("[Spotlight] Replay pressed");
#endif
        }

        private void HandleShare()
        {
            AnalyticsManager.Instance.LogEvent("artwork_shared", new Dictionary<string, object>
            {
                { "pack_id",  _packId  },
                { "image_id", _imageId }
            });
            var title = _titleText != null ? _titleText.text : "a puzzle";
#if UNITY_ANDROID && !UNITY_EDITOR
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var intent      = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intent.Call<AndroidJavaObject>("setType", "text/plain");
            intent.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"),
                $"I completed \"{title}\" on Chroma Jigsaw!");
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var chooser     = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, "Share via");
            activity.Call("startActivity", chooser);
#elif UNITY_EDITOR
            Debug.Log($"[Spotlight] Share — \"{title}\"");
#endif
        }

        private void HandleWallpaper()
        {
            AnalyticsManager.Instance.LogEvent("artwork_wallpaper_set", new Dictionary<string, object>
            {
                { "pack_id",  _packId  },
                { "image_id", _imageId }
            });
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_artworkImage == null || _artworkImage.texture == null) return;
            var tex = _artworkImage.texture as Texture2D;
            if (tex == null) return;

            byte[] png = tex.EncodeToPNG();

            var bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
            var bitmap        = bitmapFactory.CallStatic<AndroidJavaObject>(
                "decodeByteArray", png, 0, png.Length);

            var unityPlayer      = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity         = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var wallpaperManager = new AndroidJavaClass("android.app.WallpaperManager")
                .CallStatic<AndroidJavaObject>("getInstance", activity);
            wallpaperManager.Call("setBitmap", bitmap);
#elif UNITY_EDITOR
            Debug.Log("[Spotlight] Set Wallpaper — Android device only");
#endif
        }
    }
}
