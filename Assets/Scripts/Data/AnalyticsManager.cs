using System.Collections.Generic;
using UnityEngine;

namespace ChromaJigsaw.Data
{
    public class AnalyticsManager : MonoBehaviour
    {
        private static AnalyticsManager _instance;
        public static AnalyticsManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<AnalyticsManager>();
                if (_instance == null) { var go = new GameObject("[AnalyticsManager]"); _instance = go.AddComponent<AnalyticsManager>(); }
                return _instance;
            }
        }

        private const int MaxQueueSize = 200;

        private readonly Queue<(string name, Dictionary<string, object> parameters)> _queue = new();
        private bool  _sdkReady;
        private float _sessionStartTime;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            _sessionStartTime = Time.realtimeSinceStartup;
            InitFirebase();
        }

        private void Start()
        {
            LogAppOpened(Application.platform.ToString(), Application.version);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                int duration = Mathf.RoundToInt(Time.realtimeSinceStartup - _sessionStartTime);
                LogEvent("app_backgrounded",
                    new Dictionary<string, object> { { "session_duration_s", duration } });
            }
            else
            {
                _sessionStartTime = Time.realtimeSinceStartup;
                LogEvent("app_foregrounded");
            }
        }

        private void InitFirebase()
        {
            // FIREBASE_READY: uncomment when Firebase SDK is imported.
            /*
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Result == Firebase.DependencyStatus.Available)
                {
                    _sdkReady = true;
                    FlushQueue();
                }
            });
            */
        }

        public void LogScreenView(string screenName) =>
            LogEvent("screen_view", new Dictionary<string, object> { { "screen_name", screenName } });

        public void LogAppOpened(string platform, string version) =>
            LogEvent("app_opened", new Dictionary<string, object>
            {
                { "platform", platform },
                { "version",  version  }
            });

        public void LogPuzzleStarted(string packId, string puzzleId, int pieceCount, string difficulty) =>
            LogEvent("puzzle_started", new Dictionary<string, object>
            {
                { "pack_id",     packId     },
                { "puzzle_id",   puzzleId   },
                { "piece_count", pieceCount },
                { "difficulty",  difficulty }
            });

        public void LogPuzzleCompleted(string packId, string puzzleId, int pieceCount, float completionTimeSec) =>
            LogEvent("puzzle_completed", new Dictionary<string, object>
            {
                { "pack_id",           packId                               },
                { "puzzle_id",         puzzleId                             },
                { "piece_count",       pieceCount                           },
                { "completion_time_s", Mathf.RoundToInt(completionTimeSec) },
                { "star_rating",       0                                    }
            });

        public void LogPuzzleAbandoned(string packId, string puzzleId, float timeSpentSec, int piecesPlaced) =>
            LogEvent("puzzle_abandoned", new Dictionary<string, object>
            {
                { "pack_id",       packId                             },
                { "puzzle_id",     puzzleId                           },
                { "time_spent_s",  Mathf.RoundToInt(timeSpentSec)    },
                { "pieces_placed", piecesPlaced                       }
            });

        public void LogDailyCompleted(string dailyId, int pieceCount, float completionTimeSec) =>
            LogEvent("daily_completed", new Dictionary<string, object>
            {
                { "daily_id",          dailyId                              },
                { "piece_count",       pieceCount                           },
                { "completion_time_s", Mathf.RoundToInt(completionTimeSec) }
            });

        public void LogZenPassShown(string trigger) =>
            LogEvent("zen_pass_view_shown", new Dictionary<string, object> { { "trigger", trigger } });

        public void LogPackPurchase(string packId, string packName, string stage, string reason = "") =>
            LogEvent($"pack_purchase_{stage}", new Dictionary<string, object>
            {
                { "pack_id",   packId   },
                { "pack_name", packName },
                { "reason",    reason   }
            });

        public void LogAccessibilityChanged(string settingName, object value) =>
            LogEvent("accessibility_changed", new Dictionary<string, object>
            {
                { "setting_name", settingName },
                { "value",        value       }
            });

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (_sdkReady)
                SendEvent(eventName, parameters);
            else
            {
                if (_queue.Count >= MaxQueueSize) _queue.Dequeue();
                _queue.Enqueue((eventName, parameters));
            }
        }

        private void FlushQueue()
        {
            while (_queue.Count > 0)
            {
                var (name, parameters) = _queue.Dequeue();
                SendEvent(name, parameters);
            }
        }

        private void SendEvent(string eventName, Dictionary<string, object> parameters)
        {
#if UNITY_EDITOR
            Debug.Log($"[Analytics] {eventName}");
#endif
            // FIREBASE_READY: replace the log above with:
            // Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, ConvertParams(parameters));
        }
    }
}
