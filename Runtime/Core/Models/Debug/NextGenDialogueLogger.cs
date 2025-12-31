using UnityEngine;

namespace NextGenDialogue
{
    public static class NextGenDialogueLogger
    {
        private const string LoggerHeader = "[Next-Gen Dialogue] ";
        
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
