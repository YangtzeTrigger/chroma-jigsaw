using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.Audio
{
    public enum SFXType
    {
        PiecePickup,    // soft tactile lift
        PiecePlaced,    // gentle thud, resting on felt
        PieceSnap,      // satisfying soft click — locked to board
        PuzzleComplete, // warm resonant tone
        ButtonClick,    // very subtle tap
        HintUsed,       // soft chime
        CountdownTick,  // never called — no timers in this game
        Error,          // muted low tone, not alarming
        Reward,         // warm ascending tone
    }

    public enum MoodType
    {
        None,     // silence
        Menu,     // warm, unhurried — MainMenu / Gallery / Puzzles tabs
        Relaxed,  // slow, meditative — 12/24 piece solve + Sanctuary + Ambience
        Focused,  // slightly more present — 48 piece solve
        Tense,    // sparse, attentive — 96 piece solve only
        Victory,  // brief warm resolution — plays once after PuzzleComplete
    }

    public enum AudioChannel { Master, SFX, Music }

    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<AudioManager>();
                if (_instance == null) { var go = new GameObject("[AudioManager]"); _instance = go.AddComponent<AudioManager>(); }
                return _instance;
            }
        }

        [SerializeField] private AudioMixer  mixer;
        [SerializeField] private AudioClip[] sfxClips;   // indexed by SFXType — assign in Inspector
        [SerializeField] private AudioClip[] musicClips; // indexed by MoodType — assign in Inspector
        [SerializeField] private int         sfxPoolSize = 8;

        private AudioSource[] _sfxPool;
        private AudioSource   _musicA;
        private AudioSource   _musicB;
        private AudioSource   _activeMusicSource;
        private int           _poolIndex;

        private const float MinDb = -80f;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            BuildPool();
            ApplyVolumesFromSave();
        }

        private void BuildPool()
        {
            _sfxPool = new AudioSource[sfxPoolSize];
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.outputAudioMixerGroup = FindGroup("SFX");
                src.playOnAwake = false;
                _sfxPool[i] = src;
            }

            _musicA = CreateMusicSource("MusicA");
            _musicB = CreateMusicSource("MusicB");
            _activeMusicSource = _musicA;
        }

        private AudioSource CreateMusicSource(string goName)
        {
            var go  = new GameObject(goName);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = FindGroup("Music");
            src.playOnAwake = false;
            src.loop        = true;
            return src;
        }

        private AudioMixerGroup FindGroup(string groupName)
        {
            if (mixer == null) return null;
            var groups = mixer.FindMatchingGroups(groupName);
            return groups.Length > 0 ? groups[0] : null;
        }

        private void ApplyVolumesFromSave()
        {
            if (SaveManager.Instance == null || mixer == null) return;
            var data = SaveManager.Instance.Data;
            SetMixerDb("MasterVol", data.masterVolume);
            SetMixerDb("SFXVol",    data.sfxVolume);
            SetMixerDb("MusicVol",  data.musicVolume);
        }

        // ── SFX ─────────────────────────────────────────────────────────────

        public void Play(SFXType type)
        {
            int idx = (int)type;
            if (sfxClips == null || idx >= sfxClips.Length || sfxClips[idx] == null) return;
            var src = _sfxPool[_poolIndex % sfxPoolSize];
            _poolIndex++;
            src.PlayOneShot(sfxClips[idx]);
        }

        // ── Music ────────────────────────────────────────────────────────────

        public void PlayMusic(MoodType mood, float fadeDuration = 1f)
        {
            int idx = (int)mood;
            AudioClip clip = (musicClips != null && idx < musicClips.Length) ? musicClips[idx] : null;
            StopAllCoroutines();
            StartCoroutine(CrossFade(clip, fadeDuration, loop: true, onFinished: null));
        }

        // Plays without looping — calls onFinished when clip ends (used for Victory).
        public void PlayMusicOnce(MoodType mood, float fadeDuration, Action onFinished)
        {
            int idx = (int)mood;
            AudioClip clip = (musicClips != null && idx < musicClips.Length) ? musicClips[idx] : null;
            StopAllCoroutines();
            StartCoroutine(CrossFade(clip, fadeDuration, loop: false, onFinished: onFinished));
        }

        public void StopMusic(float fadeDuration = 1f)
        {
            StopAllCoroutines();
            StartCoroutine(CrossFade(null, fadeDuration, loop: false, onFinished: null));
        }

        private IEnumerator CrossFade(AudioClip incoming, float duration, bool loop, Action onFinished)
        {
            var outSrc = _activeMusicSource;
            var inSrc  = (_activeMusicSource == _musicA) ? _musicB : _musicA;
            _activeMusicSource = inSrc;

            inSrc.loop   = loop;
            inSrc.clip   = incoming;
            inSrc.volume = 0f;
            if (incoming != null) inSrc.Play();

            float elapsed  = 0f;
            float startVol = outSrc.volume;
            while (elapsed < duration)
            {
                elapsed      += Time.unscaledDeltaTime;
                float t       = Mathf.Clamp01(elapsed / duration);
                outSrc.volume = Mathf.Lerp(startVol, 0f, t);
                inSrc.volume  = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }

            outSrc.Stop();
            outSrc.volume = 0f;
            inSrc.volume  = 1f;

            if (!loop && incoming != null)
            {
                yield return new WaitWhile(() => inSrc.isPlaying);
                onFinished?.Invoke();
            }
            else
            {
                onFinished?.Invoke();
            }
        }

        // ── Volume ───────────────────────────────────────────────────────────

        // Persists to SaveData — use for user-controlled slider changes.
        public void SetVolume(AudioChannel channel, float linear)
        {
            linear = Mathf.Clamp01(linear);
            SetMixerDb(ChannelToParam(channel), linear);

            if (SaveManager.Instance == null) return;
            var data = SaveManager.Instance.Data;
            switch (channel)
            {
                case AudioChannel.Master: data.masterVolume = linear; break;
                case AudioChannel.SFX:   data.sfxVolume    = linear; break;
                case AudioChannel.Music: data.musicVolume  = linear; break;
            }
            SaveManager.Instance.Save();
        }

        // Sets mixer only — does not persist. Use for transient context overrides (e.g. Sanctuary 60% SFX).
        public void SetTransientVolume(AudioChannel channel, float linear)
        {
            linear = Mathf.Clamp01(linear);
            SetMixerDb(ChannelToParam(channel), linear);
        }

        public float GetSavedVolume(AudioChannel channel)
        {
            if (SaveManager.Instance == null) return 1f;
            var data = SaveManager.Instance.Data;
            return channel switch
            {
                AudioChannel.Master => data.masterVolume,
                AudioChannel.SFX   => data.sfxVolume,
                AudioChannel.Music => data.musicVolume,
                _                  => 1f,
            };
        }

        private static string ChannelToParam(AudioChannel channel) => channel switch
        {
            AudioChannel.Master => "MasterVol",
            AudioChannel.SFX   => "SFXVol",
            AudioChannel.Music => "MusicVol",
            _                  => "MasterVol",
        };

        private void SetMixerDb(string param, float linear)
        {
            mixer?.SetFloat(param, LinearToDb(linear));
        }

        private static float LinearToDb(float linear) =>
            linear > 0.0001f ? Mathf.Log10(linear) * 20f : MinDb;
    }
}
