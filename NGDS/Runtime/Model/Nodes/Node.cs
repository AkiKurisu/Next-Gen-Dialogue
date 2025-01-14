using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
namespace Kurisu.NGDS
{
    public abstract class Node : IDisposable
    {
        protected internal bool IsPooled { get; set; }
        
        public List<IDialogueModule> Modules { get; } = new();
        
        public void AddModule(IDialogueModule module)
        {
            Modules.Add(module);
            OnModuleAdd(module);
            if (module is IApplyable applyableModule) applyableModule.Apply(this);
        }
        
        protected void ClearModules()
        {
            Modules.Clear();
        }
        
        protected virtual void OnModuleAdd(IDialogueModule module) { }
        
        public IEnumerable<Node> ChildNodes()
        {
            return Modules.OfType<Node>();
        }
        
        public bool TryGetModule<T>(out T module) where T : IDialogueModule
        {
            foreach (var localModule in Modules)
            {
                if (localModule is T target)
                {
                    module = target;
                    return true;
                }
            }
            module = default;
            return false;
        }
        
        public void CollectModules<T>(List<T> inModules) where T : IDialogueModule
        {
            foreach (var localModule in Modules)
            {
                if (localModule is T target)
                {
                    inModules.Add(target);
                }
            }
        }

        public async UniTask ProcessModules(IObjectResolver resolver)
        {
            foreach (var localModule in Modules)
            {
                if (localModule is IProcessable processable)
                {
                    await processable.Process(resolver);
                }
            }
        }

        public void Dispose()
        {
            foreach (var childNode in ChildNodes())
            {
                childNode.Dispose();
            }
            OnDispose();
        }
        
        protected virtual void OnDispose() { }
    }
}
