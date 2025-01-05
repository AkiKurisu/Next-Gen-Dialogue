using System;
using Ceres;
using Ceres.Annotations;
using Kurisu.NGDS;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Invoke a scriptableEvent")]
    [CeresLabel("Dialogue: Invoke ScriptableEvent")]
    [NodeGroup("Dialogue")]
    public class InvokeScriptableEvent : Action
    {
        public SharedUObject<ScriptableEvent> scriptableEvent;
        
        protected override Status OnUpdate()
        {
            scriptableEvent.Value?.Invoke();
            return Status.Success;
        }
    }
}
