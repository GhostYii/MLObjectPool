using UnityEngine;
using JetBrains.Annotations;

namespace MLObjectPool
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        #region  Fields
        [CanBeNull]
        private static T _instance;

        public static bool Quitting { get; private set; }

        [NotNull]
        // ReSharper disable once StaticMemberInGenericType
        private static readonly object Lock = new object();

        public bool _persistent = false;
        #endregion

        #region  Properties
        [NotNull]
        public static T Instance
        {
            get
            {
                if (Quitting)
                {
                    Log.PrintWarning($"[{nameof(Singleton<T>)}<{typeof(T)}>] Instance will not be returned because the application is quitting.");
                    // ReSharper disable once AssignNullToNotNullAttribute
                    return null;
                }
                lock (Lock)
                {
                    if (_instance != null)
                        return _instance;
                    var instances = FindObjectsOfType<T>();
                    var count = instances.Length;
                    if (count > 0)
                    {
                        if (count == 1)
                            return _instance = instances[0];
                        Log.PrintWarning($"[{nameof(Singleton<T>)}<{typeof(T)}>] There should never be more than one {nameof(Singleton<T>)} of type {typeof(T)} in the scene, but {count} were found. The first instance found will be used, and all others will be destroyed.");
                        for (var i = 1; i < instances.Length; i++)
                            Destroy(instances[i]);
                        return _instance = instances[0];
                    }

                    Log.Print($"[{nameof(Singleton<T>)}<{typeof(T)}>] An instance is needed in the scene and no existing instances were found, so a new instance will be created.");
                    return _instance = new GameObject($"({nameof(Singleton<T>)})")
                               .AddComponent<T>();
                }
            }
        }
        #endregion

        #region  Methods
        private void Awake()
        {
            if (_persistent)
                DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        protected virtual void OnAwake() { }

        private void OnApplicationQuit()
        {
            Quitting = true;
        }
        #endregion
    }
}