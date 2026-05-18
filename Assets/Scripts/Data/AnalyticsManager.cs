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
        private bool _sdkReady;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitFirebase();
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
