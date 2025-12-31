using System;
using System.Collections.Generic;
using UnityEngine.Pool;
namespace NextGenDialogue
{
    public class Piece : Node, IContentModule, IDialogueModule
    {
        private static readonly ObjectPool<Piece> Pool = new(() => new Piece(),
            piece => piece.IsPooled = true, piece => piece.Reset());
        
        private const string DefaultID = "00";
        
        public string ID { get; set; } = DefaultID;
        
        public string[] Contents { get; private set; } = Array.Empty<string>();
        
        private readonly List<Option> _options = new();
        
        public IReadOnlyList<Option> Options => _options;
        
        private void Reset()
        {
            IsPooled = false;
            ID = DefaultID;
            Contents = Array.Empty<string>();
            _options.Clear();
            ClearModules();
        }
        
        public void AddOption(Option option)
        {
            if (_options.Contains(option)) return;
            _options.Add(option);
        }
        
        public static Piece GetPooled()
        {
            return Pool.Get();
        }
        
        protected override void OnModuleAdd(IDialogueModule module)
        {
            if (module is Option option) AddOption(option);
        }
        
        protected override void OnDispose()
        {
            if (IsPooled)
            {
                Pool.Release(this);
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