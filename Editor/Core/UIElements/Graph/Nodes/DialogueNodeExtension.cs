using Ceres;
using Ceres.Graph;
using UnityEngine;
namespace NextGenDialogue.Graph.Editor
{
    public static class DialogueNodeExtension
    {
        public static T GetFieldValue<T>(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            try
            {
                return (T)dialogueNodeView.GetFieldResolver(fieldName).Value;
            }
            catch
            {
                CeresAPI.LogError($"Can not cast field value from {fieldName} of type {typeof(T)} in {dialogueNodeView}");
                return default;
            }
        }
        
        public static string GetSharedStringValue(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            return dialogueNodeView.GetSharedVariableValue<string>(fieldName) ?? string.Empty;
        }
        
        public static int GetSharedIntValue(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            return dialogueNodeView.GetSharedVariableValue<int>(fieldName);
        }
        
        public static float GetSharedFloatValue(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            return dialogueNodeView.GetSharedVariableValue<float>(fieldName);
        }
        
        public static Vector3 GetSharedVector3Value(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            return dialogueNodeView.GetSharedVariableValue<Vector3>(fieldName);
        }
        
        public static bool GetSharedBoolValue(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            return dialogueNodeView.GetSharedVariableValue<bool>(fieldName);
        }
        
        public static Object GetSharedObjectValue(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            return dialogueNodeView.GetSharedVariableValue<Object>(fieldName);
        }
        
        public static T GetSharedVariableValue<T>(this IDialogueNodeView dialogueNodeView, string fieldName)
        {
            var sharedVariable = dialogueNodeView.GetFieldValue<SharedVariable<T>>(fieldName);
            if (sharedVariable == null)
            {
                return default;
            }

            if (!sharedVariable.IsShared)
            {
                return sharedVariable.Value;
            }

            var variable = dialogueNodeView.GraphView.GetSharedVariable<T>(sharedVariable.Name);
            if (variable == null)
            {
                return default;
            }
            return variable.Value;
        }
    }
}