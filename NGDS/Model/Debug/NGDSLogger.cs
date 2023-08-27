using UnityEngine;
namespace Kurisu.NGDS
{
    public static class NGDSLogger
    {
        private const string LoggerHeader = "[NGDS] : ";
        public static void LogWarning(string message)
        {
            Debug.LogWarning(LoggerHeader + message);
        }
        public static void Log(string message)
        {
            Debug.Log(LoggerHeader + message);
        }
        public static void LogError(string message)
        {
            Debug.LogError(LoggerHeader + message);
        }
    }
}
