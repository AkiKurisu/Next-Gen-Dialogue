using System;
using UnityEngine;
using UnityEngine.Pool;
namespace Kurisu.NGDS
{
    [Serializable]
    public class Option : Node, IContent, IDialogueModule
    {
        private static readonly ObjectPool<Option> pool = new(() => new Option(), (o) => o.IsPooled = true, (o) => o.Reset());
        [field: SerializeField]
        public string Content { get; set; } = string.Empty;
        [field: SerializeField]
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
            return pool.Get();
        }
        protected override void OnDispose()
        {
            if (IsPooled)
            {
                pool.Release(this);
            }
        }
    }
}