using System;
using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDS
{
    [Serializable]
    public class Piece : Node, IContent, IDialogueModule
    {
        private const string DefaultID = "00";
        [field: SerializeField]
        public string PieceID { get; set; } = DefaultID;
        [field: SerializeField]
        public string Content { get; set; } = string.Empty;
        private readonly List<Option> options = new();
        public IReadOnlyList<Option> Options => options;
        private Piece Reset()
        {
            PieceID = DefaultID;
            Content = string.Empty;
            options.Clear();
            ClearModules();
            return this;
        }
        public void AddOption(Option option)
        {
            if (options.Contains(option)) return;
            options.Add(option);
        }
        public static Piece CreatePiece()
        {
            return NodePoolManager.Instance.GetNode<Piece>().Reset();
        }
        protected override void OnModuleAdd(IDialogueModule module)
        {
            if (module is Option option) AddOption(option);
        }
    }
}