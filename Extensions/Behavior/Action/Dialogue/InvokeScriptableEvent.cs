using Ceres;
using Ceres.Annotations;
using Kurisu.NGDS;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Invoke a scriptableEvent")]
    [NodeLabel("Dialogue: Invoke ScriptableEvent")]
    [NodeGroup("Dialogue")]
    public class InvokeScriptableEvent : Action
    {
        public SharedTObject<ScriptableEvent> scriptableEvent;
        protected override Status OnUpdate()
        {
            scriptableEvent.Value?.Invoke();
            return Status.Success;
        }
    }
}
