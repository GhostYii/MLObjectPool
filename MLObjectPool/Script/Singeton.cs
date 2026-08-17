using UnityEngine;

namespace MLObjectPool
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object Lock = new object();

        [SerializeField] private bool _persistent;

        public static bool Quitting { get; private set; }

        public bool Persistent
        {
            get => _persistent;
            set => _persistent = value;
        }

        public static T Instance
        {
            get
            {
                if (Quitting)
                {
                    Log.PrintWarning($"[Singleton<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                    return null;
                }

                lock (Lock)
                {
                    if (_instance != null)
                        return _instance;

                    var instances = FindObjectsOfType<T>();
                    if (instances.Length > 0)
                    {
                        if (instances.Length > 1)
                        {
                            Log.PrintWarning($"[Singleton<{typeof(T)}>] There should never be more than one {typeof(T)} in the scene, but {instances.Length} were found. The first instance found will be used.");
                            for (int i = 1; i < instances.Length; i++)
                                Destroy(instances[i]);
                        }

                        return _instance = instances[0];
                    }

                    Log.Print($"[Singleton<{typeof(T)}>] An instance is needed in the scene, so a new instance will be created.");
                    return _instance = new GameObject($"[{typeof(T).Name}]").AddComponent<T>();
                }
            }
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this as T;

            if (_persistent)
                DontDestroyOnLoad(gameObject);

            Quitting = false;
            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        private void OnApplicationQuit()
        {
            Quitting = true;
        }
    }
}
