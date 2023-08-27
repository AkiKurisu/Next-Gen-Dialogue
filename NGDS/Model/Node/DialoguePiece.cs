using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDS
{
    [Serializable]
    public class DialoguePiece : DialogueNode, IContent, IDialogueModule
    {
        private const string DefaultID = "00";
        [field: SerializeField]
        public string PieceID { get; set; } = DefaultID;
        [field: SerializeField]
        public string Content { get; set; } = string.Empty;
        private readonly List<DialogueOption> options = new();
        public IReadOnlyList<DialogueOption> Options => options;
        private DialoguePiece Reset()
        {
            PieceID = DefaultID;
            Content = string.Empty;
            options.Clear();
            ClearModules();
            return this;
        }
        public void AddOption(DialogueOption option)
        {
            if (options.Contains(option)) return;
            options.Add(option);
        }
        public static DialoguePiece CreatePiece()
        {
            return NodePoolManager.Instance.GetNode<DialoguePiece>().Reset();
        }
        protected override void OnModuleAdd(IDialogueModule module)
        {
            if (module is DialogueOption option) AddOption(option);
        }
    }
}