using UnityEngine;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.Audio
{
    // Not a singleton — place one instance in MainMenu scene, one in Game scene.
    // Call SetContext() when the player's current screen or puzzle difficulty changes.
    public class MusicContextController : MonoBehaviour
    {
        public enum GameContext
        {
            MainMenu,
            PuzzleSolve_Easy,    // 12 or 24 pieces → Relaxed
            PuzzleSolve_Medium,  // 48 pieces → Focused
            PuzzleSolve_Hard,    // 96 pieces → Tense
            PuzzleComplete,      // Victory plays once, then returns to previous mood
            Sanctuary,           // Relaxed at 60% SFX volume
            Ambience,            // Relaxed
        }

        private MoodType     _currentMood    = MoodType.None;
        private MoodType     _priorMood      = MoodType.None;
        private GameContext  _currentContext = GameContext.MainMenu;
        private float        _savedSfxVolume = 1f;
        private bool         _sanctuaryActive;

        private void Start() => SetContext(GameContext.MainMenu);

        public void SetContext(GameContext context)
        {
            if (context == _currentContext) return;
            _currentContext = context;

            // Leave Sanctuary — restore SFX volume before switching mood.
            if (_sanctuaryActive && context != GameContext.Sanctuary)
            {
                _sanctuaryActive = false;
                AudioManager.Instance.SetTransientVolume(AudioChannel.SFX, _savedSfxVolume);
            }

            switch (context)
            {
                case GameContext.MainMenu:
                    SetMood(MoodType.Menu);
                    break;

                case GameContext.PuzzleSolve_Easy:
                    SetMood(MoodType.Relaxed);
                    break;

                case GameContext.PuzzleSolve_Medium:
                    SetMood(MoodType.Focused);
                    break;

                case GameContext.PuzzleSolve_Hard:
                    SetMood(MoodType.Tense);
                    break;

                case GameContext.PuzzleComplete:
                    _priorMood = _currentMood;
                    AudioManager.Instance.PlayMusicOnce(MoodType.Victory, 1f, OnVictoryFinished);
                    _currentMood = MoodType.Victory;
                    break;

                case GameContext.Sanctuary:
                    _savedSfxVolume  = AudioManager.Instance.GetSavedVolume(AudioChannel.SFX);
                    _sanctuaryActive = true;
                    AudioManager.Instance.PlayMusic(MoodType.Relaxed, 3f);
                    AudioManager.Instance.SetTransientVolume(AudioChannel.SFX, 0.6f);
                    _currentMood = MoodType.Relaxed;
                    break;

                case GameContext.Ambience:
                    SetMood(MoodType.Relaxed);
                    break;
            }
        }

        private void SetMood(MoodType mood)
        {
            if (_currentMood == mood) return;
            _currentMood = mood;
            AudioManager.Instance.PlayMusic(mood);
        }

        private void OnVictoryFinished()
        {
            _currentMood = _priorMood;
            AudioManager.Instance.PlayMusic(_priorMood);
        }
    }
}
