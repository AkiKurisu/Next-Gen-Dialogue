using System.Collections.Generic;
using UnityEngine.Pool;
namespace NextGenDialogue
{
    public class Option : Node, IContentModule, IDialogueModule
    {
        private static readonly ObjectPool<Option> Pool = new(() => new Option(), o => o.IsPooled = true, (o) => o.Reset());
        
        public int Index { get; set; }
        
        public string Content { get; set; } = string.Empty;
        
        public string TargetID { get; set; } = string.Empty;
        
        private Option Reset()
        {
            IsPooled = false;
            Content = string.Empty;
            TargetID = string.Empty;
            ClearModules();
            return this;
        }
        
        public static Option GetPooled()
        {
            return Pool.Get();
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
            contents.Add(Content);
        }
        
        public void AddContent(string content)
        {
            Content = content;
        }
        
        public void SetContents(List<string> contents)
        {
            Content = contents.Count > 0 ? contents[0] : string.Empty;
        }
    }
}