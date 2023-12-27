using System.Collections.Generic;
using System.Linq;
namespace Kurisu.NGDS
{
    public abstract class Node
    {
        private readonly List<IDialogueModule> modules = new();
        public IReadOnlyList<IDialogueModule> Modules => modules;
        public void AddModule(IDialogueModule module)
        {
            modules.Add(module);
            OnModuleAdd(module);
            if (module is IApplyable applyableModule) applyableModule.Apply(this);
        }
        protected void ClearModules()
        {
            modules.Clear();
        }
        protected virtual void OnModuleAdd(IDialogueModule module) { }
        public IEnumerable<Node> ChildNodes()
        {
            return modules.OfType<Node>();
        }
        public bool TryGetModule<T>(out T module) where T : IDialogueModule
        {
            foreach (var localModule in modules)
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
        public List<T> CollectModules<T>(List<T> modules) where T : IDialogueModule
        {
            foreach (var localModule in this.modules)
            {
                if (localModule is T target)
                {
                    modules.Add(target);
                }
            }
            return modules;
        }
    }
}
