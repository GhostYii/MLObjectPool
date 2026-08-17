using UnityEngine;

namespace MLObjectPool.Editor
{
    internal static class Log
    {
        private static bool _logEnable = true;

        public static bool LogEnable
        {
            get => _logEnable;
            set => _logEnable = value;
        }

        public static void Print(string msg)
        {
            if (_logEnable)
                Debug.Log($"{Constant.DEBUG_NAME}: {msg}");
        }

        public static void PrintError(string msg)
        {
            if (_logEnable)
                Debug.LogError($"{Constant.DEBUG_NAME}: {msg}");
        }

        public static void PrintWarning(string msg)
        {
            if (_logEnable)
                Debug.LogWarning($"{Constant.DEBUG_NAME}: {msg}");
        }
    }
}
