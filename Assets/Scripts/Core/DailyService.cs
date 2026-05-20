using System;
using UnityEngine;

namespace ChromaJigsaw.Core
{
    public enum DailyState { NotStarted, InProgress, Completed }

    public class DailyService : MonoBehaviour
    {
        private static DailyService _instance;
        public static DailyService Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<DailyService>();
                if (_instance == null) { var go = new GameObject("[DailyService]"); _instance = go.AddComponent<DailyService>(); }
                return _instance;
            }
        }

        private DailyManifest _manifest;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialise(DailyManifest manifest)
        {
            _manifest = manifest;
        }

        public DailyEntry[] GetActiveDailies() =>
            _manifest?.entries ?? Array.Empty<DailyEntry>();

        public DailyState GetDailyState(string dailyId)
        {
            var rec = SaveManager.Instance.Data.dailyRecords.Find(r => r.dailyId == dailyId);
            if (rec == null)          return DailyState.NotStarted;
            if (rec.completed)        return DailyState.Completed;
            if (rec.piecesSolved > 0) return DailyState.InProgress;
            return DailyState.NotStarted;
        }

        public void SaveProgress(string dailyId, int piecesSolved)
        {
            var rec           = GetOrCreateRecord(dailyId);
            rec.piecesSolved  = piecesSolved;
            rec.lastPlayedUtc = DateTime.UtcNow.ToString("o");
            SaveManager.Instance.Save();
        }

        public void MarkComplete(string dailyId)
        {
            var rec           = GetOrCreateRecord(dailyId);
            rec.completed     = true;
            rec.lastPlayedUtc = DateTime.UtcNow.ToString("o");
            SaveManager.Instance.Save();
        }

        private DailyRecord GetOrCreateRecord(string dailyId)
        {
            var data = SaveManager.Instance.Data;
            var rec  = data.dailyRecords.Find(r => r.dailyId == dailyId);
            if (rec == null)
            {
                rec = new DailyRecord { dailyId = dailyId };
                data.dailyRecords.Add(rec);
            }
            return rec;
        }
    }
}
