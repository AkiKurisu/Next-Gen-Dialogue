using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueNodeExtension
    {
        public static T GetFieldValue<T>(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return (T)dialogueTreeNode.GetFieldResolver(fieldName).Value;
        }
        public static string GetSharedStringValue(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<string>(fieldName) ?? string.Empty;
        }
        public static int GetSharedIntValue(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<int>(fieldName);
        }
        public static float GetSharedFloatValue(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<float>(fieldName);
        }
        public static Vector3 GetSharedVector3Value(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<Vector3>(fieldName);
        }
        public static bool GetSharedBoolValue(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<bool>(fieldName);
        }
        public static Object GetSharedObjectValue(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<Object>(fieldName);
        }
        public static T GetSharedVariableValue<T>(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            var sharedVariable = dialogueTreeNode.GetSharedVariable<SharedVariable<T>>(fieldName);
            return sharedVariable != null ? dialogueTreeNode.MapTreeView.GetSharedVariableValue(sharedVariable) : default;
        }
        public static T GetSharedVariable<T>(this IDialogueNode dialogueTreeNode, string fieldName) where T : SharedVariable
        {
            try
            {
                return dialogueTreeNode.GetFieldResolver(fieldName).Value as T;
            }
            catch
            {
                Debug.Log($"Can not cast variable from {fieldName} for {typeof(T)} in {dialogueTreeNode}");
                return null;
            }
        }
    }
}