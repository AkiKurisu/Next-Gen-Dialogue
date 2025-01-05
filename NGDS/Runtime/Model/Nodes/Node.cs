using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
namespace Kurisu.NGDS
{
    public abstract class Node : IDisposable
    {
        protected internal bool IsPooled { get; set; }
        
        private readonly List<IDialogueModule> _modules = new();
        
        public IReadOnlyList<IDialogueModule> Modules => _modules;
        
        public void AddModule(IDialogueModule module)
        {
            _modules.Add(module);
            OnModuleAdd(module);
            if (module is IApplyable applyableModule) applyableModule.Apply(this);
        }
        
        protected void ClearModules()
        {
            _modules.Clear();
        }
        
        protected virtual void OnModuleAdd(IDialogueModule module) { }
        
        public IEnumerable<Node> ChildNodes()
        {
            return _modules.OfType<Node>();
        }
        
        public bool TryGetModule<T>(out T module) where T : IDialogueModule
        {
            foreach (var localModule in _modules)
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
            foreach (var localModule in _modules)
            {
                if (localModule is T target)
                {
                    inModules.Add(target);
                }
            }
        }

        public async UniTask ProcessModules(IObjectResolver resolver)
        {
            foreach (var localModule in _modules)
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
