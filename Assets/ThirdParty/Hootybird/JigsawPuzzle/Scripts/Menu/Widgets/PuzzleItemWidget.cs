using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Widget displaying puzzle image and available puzzle settings.
    /// </summary>
    public class PuzzleItemWidget : MonoBehaviour
    {
        [SerializeField]
        private RawImage icon;
        [SerializeField]
        private PuzzleSettingWidget puzzleSettingsPrefab;
        [SerializeField]
        private RectTransform settingsParent;

        private Button button;
        private AspectRatioFitter iconAspectRatioFitter;
        private List<PuzzleSettingWidget> puzzleSettingsWidgets = new List<PuzzleSettingWidget>();

        public PuzzleInfoObject PuzzleInfo { get; private set; }
        public int PuzzleSeed => PuzzleInfo.Seed;
        public string PuzzleId => PuzzleInfo.Id;

        private void Awake()
        {
            iconAspectRatioFitter = icon.GetComponent<AspectRatioFitter>();

            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        public void SetPuzzleInfo(PuzzleInfoObject puzzleInfo)
        {
            PuzzleInfo = puzzleInfo;

            icon.texture = puzzleInfo.PuzzleTexture;
            iconAspectRatioFitter.aspectRatio = (float)icon.texture.width / icon.texture.height;
            LoadPuzzleSettings(puzzleInfo);
        }

        public void LoadPuzzleSettings(PuzzleInfoObject puzzleInfo)
        {
            foreach (PuzzleSettingWidget puzzleSettingWidget in puzzleSettingsWidgets)
            {
                Destroy(puzzleSettingWidget.gameObject);
            }

            foreach (PuzzleSettingsObject puzzleSettingObject in puzzleInfo.Options)
            {
                PuzzleSettingWidget puzzleSettingWidget = Instantiate(puzzleSettingsPrefab, settingsParent);
                puzzleSettingWidget.SetPuzzleSettings(puzzleSettingObject);

                puzzleSettingsWidgets.Add(puzzleSettingWidget);
            }
        }

        private void OnButtonClick()
        {
            if (puzzleSettingsWidgets.Count > 0)
            {
                puzzleSettingsWidgets[0].OpenPuzzle();
            }
        }
    }
}
