using System;
using UnityEngine;
namespace Kurisu.NGDS
{
    [Serializable]
    public class Option : Node, IContent, IDialogueModule
    {
        [field: SerializeField]
        public string Content { get; set; } = string.Empty;
        [field: SerializeField]
        public string TargetID { get; set; } = string.Empty;
        private Option Reset()
        {
            Content = string.Empty;
            TargetID = string.Empty;
            ClearModules();
            return this;
        }
        public static Option CreateOption()
        {
            return NodePoolManager.Instance.GetNode<Option>().Reset();
        }
    }
}