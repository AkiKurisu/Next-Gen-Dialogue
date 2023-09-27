using System;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueNodeExtension
    {
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
        public static UnityEngine.Object GetSharedObjectValue(this IDialogueNode dialogueTreeNode, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<UnityEngine.Object>(fieldName);
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
                return (T)dialogueTreeNode.GetFieldResolver(fieldName).Value;
            }
            catch (Exception e)
            {
                Debug.Log(e);
                return null;
            }
        }
    }
}