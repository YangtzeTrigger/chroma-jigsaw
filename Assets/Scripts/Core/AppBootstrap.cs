using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChromaJigsaw.Core
{
    public class AppBootstrap : MonoBehaviour
    {
        private static AppBootstrap _instance;
        public static AppBootstrap Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<AppBootstrap>();
                if (_instance == null) { var go = new GameObject("[AppBootstrap]"); _instance = go.AddComponent<AppBootstrap>(); }
                return _instance;
            }
        }

        [SerializeField] private DailyManifest _dailyManifest;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(Boot());
        }

        private IEnumerator Boot()
        {
            // Script Execution Order guarantees all singleton Awake()s have fired
            // before this coroutine body runs after yield.
            yield return null;
            ZenPassService.Instance.Initialise();
            SubscriptionService.Instance.Initialise();
            IAPManager.Instance.InitializeIAP();
            DailyService.Instance.Initialise(_dailyManifest);
            SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}
