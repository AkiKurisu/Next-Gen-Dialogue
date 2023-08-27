using System;
using UnityEngine;
namespace Kurisu.NGDS
{
    [Serializable]
    public class DialogueOption : DialogueNode, IContent, IDialogueModule
    {
        [field: SerializeField]
        public string Content { get; set; } = string.Empty;
        [field: SerializeField]
        public string TargetID { get; set; } = string.Empty;
        private DialogueOption Reset()
        {
            Content = string.Empty;
            TargetID = string.Empty;
            ClearModules();
            return this;
        }
        public static DialogueOption CreateOption()
        {
            return NodePoolManager.Instance.GetNode<DialogueOption>().Reset();
        }
    }
}