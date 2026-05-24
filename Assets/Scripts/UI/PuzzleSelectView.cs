using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class PuzzleSelectView : MonoBehaviour
    {
        // Assign any test Texture2D in the Inspector (e.g. a Hootybird sample image).
        // Falls back to a solid-colour swatch if left empty.
        [SerializeField] private Texture2D _testTexture;

        private bool         _isBuilt;
        private string       _packId;
        private PackConfigSO _config;
        private int          _selectedPieceCount = 24;

        private TextMeshProUGUI    _packNameText;
        private readonly Image[]           _pieceCountImgs   = new Image[4];
        private readonly TextMeshProUGUI[] _pieceCountLabels = new TextMeshProUGUI[4];

        private static readonly int[]  PieceCounts = { 12, 24, 48, 96 };
        private static readonly Color  ColPanel    = new Color(0.075f, 0.075f, 0.075f);
        private static readonly Color  ColGold     = new Color(0.957f, 0.741f, 0.380f);
        private static readonly Color  ColText     = new Color(0.957f, 0.886f, 0.882f);
        private static readonly Color  ColBorder   = new Color(0.310f, 0.275f, 0.216f);

        public void Open(string packId, PackConfigSO config)
        {
            if (!_isBuilt) { BuildUI(); _isBuilt = true; }

            _packId             = packId;
            _config             = config;
            _selectedPieceCount = 24;
            gameObject.SetActive(true);

            if (_packNameText != null)
                _packNameText.text = config?.packName ?? packId;

            RefreshPieceCountButtons();
            AnalyticsManager.Instance.LogScreenView("puzzle_select");
        }

        public void Close() => gameObject.SetActive(false);

        private void SelectPieceCount(int count)
        {
            _selectedPieceCount = count;
            RefreshPieceCountButtons();
        }

        private void RefreshPieceCountButtons()
        {
            for (int i = 0; i < PieceCounts.Length; i++)
            {
                bool selected = PieceCounts[i] == _selectedPieceCount;
                if (_pieceCountImgs[i]   != null) _pieceCountImgs[i].color   = selected ? ColGold  : ColBorder;
                if (_pieceCountLabels[i] != null) _pieceCountLabels[i].color = selected ? ColPanel : ColText;
            }
        }

        private static Texture2D MakeSolidTexture()
        {
            var tex    = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            var fill   = new Color(0.22f, 0.47f, 0.65f);
            var pixels = new Color[512 * 512];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = fill;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private void Begin()
        {
            var tex = _testTexture != null ? _testTexture : MakeSolidTexture();

            string puzzleId = _config?.imageIds != null && _config.imageIds.Length > 0
                ? _config.imageIds[0]
                : $"{_packId}_img_000";

            PuzzleController.SessionPackId     = _packId;
            PuzzleController.SessionPuzzleId   = puzzleId;
            PuzzleController.SessionPieceCount = _selectedPieceCount;
            PuzzleController.SessionIsDaily    = false;
            PuzzleController.SessionTexture    = tex;

            AnalyticsManager.Instance.LogEvent("puzzle_begin_tapped", new Dictionary<string, object>
            {
                { "pack_id",     _packId             },
                { "puzzle_id",   puzzleId            },
                { "piece_count", _selectedPieceCount }
            });

            DOTween.KillAll();
            SceneManager.LoadScene(SceneNames.Game);
        }

        private void BuildUI()
        {
            var rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }

            // Full-screen backdrop — tap to dismiss
            var backdrop = new GameObject("Backdrop");
            backdrop.transform.SetParent(transform, false);
            var bdImg = backdrop.AddComponent<Image>();
            bdImg.color = new Color(0f, 0f, 0f, 0.72f);
            var bdRT = backdrop.GetComponent<RectTransform>();
            bdRT.anchorMin = Vector2.zero;
            bdRT.anchorMax = Vector2.one;
            bdRT.offsetMin = bdRT.offsetMax = Vector2.zero;
            var bdBtn = backdrop.AddComponent<Button>();
            bdBtn.targetGraphic = bdImg;
            bdBtn.onClick.AddListener(Close);

            // Center card
            var card    = new GameObject("Card");
            card.transform.SetParent(transform, false);
            var cardImg = card.AddComponent<Image>();
            cardImg.color = ColPanel;
            var cardRT  = card.GetComponent<RectTransform>();
            cardRT.anchorMin = new Vector2(0.08f, 0.28f);
            cardRT.anchorMax = new Vector2(0.92f, 0.72f);
            cardRT.offsetMin = cardRT.offsetMax = Vector2.zero;

            // Pack name label
            var nameGO    = new GameObject("PackName");
            nameGO.transform.SetParent(card.transform, false);
            _packNameText = nameGO.AddComponent<TextMeshProUGUI>();
            _packNameText.fontSize  = 22;
            _packNameText.alignment = TextAlignmentOptions.Center;
            _packNameText.color     = ColText;
            var nameRT = nameGO.GetComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.05f, 0.76f);
            nameRT.anchorMax = new Vector2(0.95f, 0.96f);
            nameRT.offsetMin = nameRT.offsetMax = Vector2.zero;

            // "PIECE COUNT" sublabel
            var subGO  = new GameObject("SubLabel");
            subGO.transform.SetParent(card.transform, false);
            var subTxt = subGO.AddComponent<TextMeshProUGUI>();
            subTxt.text      = "PIECE COUNT";
            subTxt.fontSize  = 11;
            subTxt.alignment = TextAlignmentOptions.Center;
            subTxt.color     = ColGold;
            var subRT = subGO.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0.05f, 0.60f);
            subRT.anchorMax = new Vector2(0.95f, 0.72f);
            subRT.offsetMin = subRT.offsetMax = Vector2.zero;

            // 12 / 24 / 48 / 96 selector buttons
            float[] xs = { 0.06f, 0.29f, 0.52f, 0.75f };
            for (int i = 0; i < PieceCounts.Length; i++)
            {
                int   pc  = PieceCounts[i];
                int   idx = i;
                float x0  = xs[i];
                float x1  = x0 + 0.19f;

                var btnGO = new GameObject($"PieceCount_{pc}");
                btnGO.transform.SetParent(card.transform, false);
                var img = btnGO.AddComponent<Image>();
                img.color = ColBorder;
                var btnRT = btnGO.GetComponent<RectTransform>();
                btnRT.anchorMin = new Vector2(x0, 0.40f);
                btnRT.anchorMax = new Vector2(x1, 0.58f);
                btnRT.offsetMin = btnRT.offsetMax = Vector2.zero;
                var btn = btnGO.AddComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() => SelectPieceCount(pc));
                _pieceCountImgs[idx] = img;

                var lblGO = new GameObject("Label");
                lblGO.transform.SetParent(btnGO.transform, false);
                var lbl = lblGO.AddComponent<TextMeshProUGUI>();
                lbl.text      = pc.ToString();
                lbl.fontSize  = 17;
                lbl.alignment = TextAlignmentOptions.Center;
                lbl.color     = ColText;
                var lblRT = lblGO.GetComponent<RectTransform>();
                lblRT.anchorMin = Vector2.zero;
                lblRT.anchorMax = Vector2.one;
                lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
                _pieceCountLabels[idx] = lbl;
            }

            // BEGIN button
            var beginGO  = new GameObject("BeginButton");
            beginGO.transform.SetParent(card.transform, false);
            var beginImg = beginGO.AddComponent<Image>();
            beginImg.color = ColGold;
            var beginRT  = beginGO.GetComponent<RectTransform>();
            beginRT.anchorMin = new Vector2(0.18f, 0.08f);
            beginRT.anchorMax = new Vector2(0.82f, 0.30f);
            beginRT.offsetMin = beginRT.offsetMax = Vector2.zero;
            var beginBtn = beginGO.AddComponent<Button>();
            beginBtn.targetGraphic = beginImg;
            beginBtn.onClick.AddListener(Begin);

            var bLblGO = new GameObject("Label");
            bLblGO.transform.SetParent(beginGO.transform, false);
            var bLbl   = bLblGO.AddComponent<TextMeshProUGUI>();
            bLbl.text      = "BEGIN";
            bLbl.fontSize  = 20;
            bLbl.fontStyle = FontStyles.Bold;
            bLbl.alignment = TextAlignmentOptions.Center;
            bLbl.color     = ColPanel;
            var bLblRT = bLblGO.GetComponent<RectTransform>();
            bLblRT.anchorMin = Vector2.zero;
            bLblRT.anchorMax = Vector2.one;
            bLblRT.offsetMin = bLblRT.offsetMax = Vector2.zero;

            RefreshPieceCountButtons();
        }
    }
}
