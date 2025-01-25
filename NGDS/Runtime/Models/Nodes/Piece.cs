using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace Kurisu.NGDS
{
    public class Piece : Node, IContentModule, IDialogueModule
    {
        private static readonly ObjectPool<Piece> pool = new(() => new Piece(), (p) => p.IsPooled = true, (p) => p.Reset());
        
        private const string DefaultID = "00";
        
        public string Name { get; set; } = string.Empty;
        
        public string ID { get; set; } = DefaultID;
        
        public string[] Contents { get; set; } = EmptyContents;
        
        private static readonly string[] EmptyContents = Array.Empty<string>();
        
        private readonly List<Option> _options = new();
        
        public IReadOnlyList<Option> Options => _options;
        
        private Piece Reset()
        {
            IsPooled = false;
            ID = DefaultID;
            Contents = EmptyContents;
            _options.Clear();
            ClearModules();
            return this;
        }
        
        public void AddOption(Option option)
        {
            if (_options.Contains(option)) return;
            _options.Add(option);
        }
        
        public static Piece GetPooled()
        {
            return pool.Get();
        }
        
        protected override void OnModuleAdd(IDialogueModule module)
        {
            if (module is Option option) AddOption(option);
        }
        
        protected override void OnDispose()
        {
            if (IsPooled)
            {
                pool.Release(this);
            }
        }

        public void GetContents(List<string> contents)
        {
            contents.AddRange(Contents);
        }

        public void SetContents(List<string> contents)
        {
            Contents = contents.ToArray();
        }

        public void AddContent(string content)
        {
            var contents = Contents;
            Array.Resize(ref contents, contents.Length + 1);
            contents[^1] = content;
            Contents = contents;
        }
    }
}