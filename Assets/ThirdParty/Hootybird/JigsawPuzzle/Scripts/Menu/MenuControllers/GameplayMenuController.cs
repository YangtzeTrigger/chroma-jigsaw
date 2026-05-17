using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.Repositories;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;
using HootyBird.JigsawPuzzleEngine.Tween;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Gameplay menu controller. Active during gameplay.
    /// </summary>
    public class GameplayMenuController : MenuController
    {
        /// <summary>
        /// Delay after which nextButton sets active.
        /// </summary>
        private static float PlayNextPuzzleDelay = 1f;

        [SerializeField]
        private Puzzle puzzle;
        [SerializeField]
        private Button nextButton;

        private PuzzleOverlay puzzleOverlay;
        private CanvasGroup nextButtonCanvasGroup;
        private TweenBase nextButtonTween;

        private Coroutine playNextRoutine;

        public PuzzleDataPackage CurrentPuzzle { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            Settings.InternalAppSettings.GameplayMenuControllerName = name;
            Application.targetFrameRate = Settings.InternalAppSettings.TargetFramerate;

            puzzleOverlay = GetOverlay<PuzzleOverlay>();
            puzzleOverlay.OnPuzzleComplete += OnPuzzleComplete;

            nextButtonCanvasGroup = nextButton.GetComponent<CanvasGroup>();
            nextButtonTween = nextButton.GetComponent<TweenBase>();
            nextButton.onClick.AddListener(NextButtonOnClick);
        }

        private void OnDisable()
        {
            puzzle.Clear();

            // Stop nextButtonRoutine if back button is pressed.
            if (playNextRoutine != null)
            {
                StopCoroutine(playNextRoutine);
            }
        }

        public void StartPuzzle(PuzzleSettingsPackage puzzlePackage, bool tryLoad)
        {
            // Disable next button;
            nextButtonCanvasGroup.blocksRaycasts = false;
            nextButtonCanvasGroup.interactable = false;
            nextButtonTween.StopTween(true);

            CurrentPuzzle = puzzlePackage.ToPuzzleDataPackage();
            string savedGameFilename = PuzzleTools.CombineSettingsWithId(
                CurrentPuzzle.puzzleData.settingsId,
                CurrentPuzzle.puzzleData.puzzleId);

            SavedGameData saveGameData = tryLoad ? SaveGameService.LoadSavedGameData(savedGameFilename) : null;

            // Set settings.
            puzzle.FreeSnapEnabled = SettingsService.GetSettingValue(SettingsOptions.FreeSnap);
            puzzle.PiecesRotationEnabled = SettingsService.GetSettingValue(SettingsOptions.Rotation);
            puzzle.ShuffleOnInitialized = SettingsService.GetSettingValue(SettingsOptions.ShufflePieces);

            // Load puzzle in.
            puzzle.Initialize(CurrentPuzzle, saveGameData, Settings.PuzzleSettings.PuzzlePieceBoardSize);
        }

        /// <summary>
        /// Open <see cref="MainMenuController"/>.
        /// </summary>
        public void OpenMainMenu()
        {
            SetActive(Settings.InternalAppSettings.MainMenuControllerName, true);
        }

        /// <summary>
        /// Invoked when puzzle complete.
        /// </summary>
        private void OnPuzzleComplete()
        {
            // Delete save file if have one.
            SaveGameService.DeleteSavedGameData(PuzzleTools.CombineSettingsWithId(
                CurrentPuzzle.puzzleData.settingsId, 
                CurrentPuzzle.puzzleData.puzzleId));

            playNextRoutine = StartCoroutine(PuzzleCompleteRoutine());
        }

        /// <summary>
        /// Invoked when nextButton is pressed.
        /// </summary>
        private void NextButtonOnClick()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);
            nextButtonTween.PlayBackward(true);

            PuzzleInfoObject randomPuzzleInfoObject = DataHandler.Instance.CategoryRepository.GetRandomPuzzleInfoObject();
            StartPuzzle(new PuzzleSettingsPackage()
            {
                puzzleTexture = randomPuzzleInfoObject.PuzzleTexture,
                puzzleId = randomPuzzleInfoObject.Id,
                seed = randomPuzzleInfoObject.Seed,
                puzzleSettings = randomPuzzleInfoObject.Options[Random.Range(0, randomPuzzleInfoObject.Options.Count)].PuzzleSettings,
            }, 
            true);
        }

        private IEnumerator PuzzleCompleteRoutine()
        {
            yield return new WaitForSeconds(PlayNextPuzzleDelay);

            nextButtonCanvasGroup.blocksRaycasts = true;
            nextButtonCanvasGroup.interactable = true;
            nextButtonTween.PlayForward(true);

            playNextRoutine = null;
        }
    }
}
