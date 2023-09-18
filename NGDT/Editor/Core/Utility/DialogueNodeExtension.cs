using System;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueNodeExtension
    {
        public static string GetSharedStringValue(this IDialogueNode dialogueTreeNode, IDialogueTreeView treeView, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<string>(treeView, fieldName) ?? string.Empty;
        }
        public static int GetSharedIntValue(this IDialogueNode dialogueTreeNode, IDialogueTreeView treeView, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<int>(treeView, fieldName);
        }
        public static float GetSharedFloatValue(this IDialogueNode dialogueTreeNode, IDialogueTreeView treeView, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<float>(treeView, fieldName);
        }
        public static Vector3 GetSharedVector3Value(this IDialogueNode dialogueTreeNode, IDialogueTreeView treeView, string fieldName)
        {
            return dialogueTreeNode.GetSharedVariableValue<Vector3>(treeView, fieldName);
        }
        public static T GetSharedVariableValue<T>(this IDialogueNode dialogueTreeNode, IDialogueTreeView treeView, string fieldName)
        {
            var sharedVariable = dialogueTreeNode.GetSharedVariable<SharedVariable<T>>(fieldName);
            return sharedVariable != null ? treeView.GetSharedVariableValue(sharedVariable) : default;
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