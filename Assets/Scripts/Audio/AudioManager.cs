using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.Audio
{
    public enum SFXType
    {
        PiecePickup,
        PieceSnap,
        PieceWrongSnap,
        PuzzleComplete,
        ButtonTap,
        ButtonBack,
    }

    public enum MoodType
    {
        None,
        Menu,
        Puzzle,
        Celebration,
    }

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

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioClip[] sfxClips;   // indexed by SFXType
        [SerializeField] private AudioClip[] musicClips; // indexed by MoodType
        [SerializeField] private int sfxPoolSize = 8;

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
            SetMixerVolume("MasterVol", data.masterVolume);
            SetMixerVolume("SFXVol",    data.sfxVolume);
            SetMixerVolume("MusicVol",  data.musicVolume);
        }

        public void PlaySFX(SFXType type)
        {
            int idx = (int)type;
            if (sfxClips == null || idx >= sfxClips.Length || sfxClips[idx] == null) return;
            var src = _sfxPool[_poolIndex % sfxPoolSize];
            _poolIndex++;
            src.PlayOneShot(sfxClips[idx]);
        }

        public void PlayMusic(MoodType mood, float fadeDuration = 1f)
        {
            int idx = (int)mood;
            AudioClip clip = (musicClips != null && idx < musicClips.Length) ? musicClips[idx] : null;
            StartCoroutine(CrossFade(clip, fadeDuration));
        }

        private IEnumerator CrossFade(AudioClip incoming, float duration)
        {
            var outSrc = _activeMusicSource;
            var inSrc  = (_activeMusicSource == _musicA) ? _musicB : _musicA;
            _activeMusicSource = inSrc;

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
        }

        public void SetVolume(string channel, float linear)
        {
            linear = Mathf.Clamp01(linear);
            SetMixerVolume(channel, linear);

            if (SaveManager.Instance == null) return;
            var data = SaveManager.Instance.Data;
            switch (channel)
            {
                case "MasterVol": data.masterVolume = linear; break;
                case "SFXVol":    data.sfxVolume    = linear; break;
                case "MusicVol":  data.musicVolume  = linear; break;
            }
            SaveManager.Instance.Save();
        }

        private void SetMixerVolume(string param, float linear)
        {
            mixer?.SetFloat(param, LinearToDb(linear));
        }

        private static float LinearToDb(float linear) =>
            linear > 0.0001f ? Mathf.Log10(linear) * 20f : MinDb;
    }
}
