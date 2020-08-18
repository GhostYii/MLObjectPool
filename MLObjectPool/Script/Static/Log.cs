namespace MLObjectPool
{
    internal static class Log
    {
        public static void Print(string msg)
        {
            UnityEngine.Debug.LogFormat("{0}: {1}", Constance.DEBUG_NAME, msg);
        }

        public static void PrintError(string msg)
        {
            UnityEngine.Debug.LogErrorFormat("{0}: {1}", Constance.DEBUG_NAME, msg);
        }

        public static void PrintWarning(string msg)
        {
            UnityEngine.Debug.LogWarningFormat("{0}: {1}", Constance.DEBUG_NAME, msg);
        }
    }
}
