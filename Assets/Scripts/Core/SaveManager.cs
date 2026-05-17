using System;
using System.IO;
using UnityEngine;

namespace ChromaJigsaw.Core
{
    [Serializable]
    public class AccessibilityPrefs
    {
        public bool highContrast = false;
        public bool largeText    = false;
    }

    [Serializable]
    public class SaveData
    {
        public float masterVolume        = 1f;
        public float sfxVolume           = 1f;
        public float musicVolume         = 0.8f;
        public AccessibilityPrefs accessibilityPrefs = new AccessibilityPrefs();
    }

    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<SaveManager>();
                if (_instance == null) { var go = new GameObject("[SaveManager]"); _instance = go.AddComponent<SaveManager>(); }
                return _instance;
            }
        }

        public SaveData Data { get; private set; }

        private string SavePath => Path.Combine(Application.persistentDataPath, "chroma_save.json");
        private string TmpPath  => SavePath + ".tmp";
        private string BakPath  => SavePath + ".bak";

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Load()
        {
            if (File.Exists(SavePath))
            {
                try   { Data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)); }
                catch { Data = new SaveData(); }
            }
            else
            {
                Data = new SaveData();
            }
        }

        public void Save()
        {
            string json = JsonUtility.ToJson(Data, true);
            File.WriteAllText(TmpPath, json);
            if (File.Exists(BakPath)) File.Delete(BakPath);
            if (File.Exists(SavePath)) File.Move(SavePath, BakPath);
            File.Move(TmpPath, SavePath);
        }
    }
}
