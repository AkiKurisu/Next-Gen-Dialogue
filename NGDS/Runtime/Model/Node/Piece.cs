using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.NGDS
{
    [Serializable]
    public class Piece : Node, IContent, IDialogueModule
    {
        private static readonly ObjectPool<Piece> pool = new(() => new Piece(), null, (p) => p.Reset());
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
        public static Piece GetPooled()
        {
            return pool.Get();
        }
        protected override void OnModuleAdd(IDialogueModule module)
        {
            if (module is Option option) AddOption(option);
        }
        public override void Dispose()
        {
            pool.Release(this);
        }
    }
}