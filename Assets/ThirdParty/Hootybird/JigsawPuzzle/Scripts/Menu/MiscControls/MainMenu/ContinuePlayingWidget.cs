using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using TMPro;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Continue playing widgets are located under "Continue Playing" section of HomeTab (if any saved games available).
    /// </summary>
    public class ContinuePlayingWidget : PlayRadomWidget
    {
        [SerializeField]
        private TMP_Text progressLabel;

        protected override bool LoadSavedGame { get; } = true;

        public void UpdateData(PuzzleInfoObject puzzleInfoObject, PuzzleSettings puzzleSetting, int progress)
        {
            progressLabel.text = $"{progress}%";

            UpdateData(puzzleInfoObject, puzzleSetting);
        }
    }
}
