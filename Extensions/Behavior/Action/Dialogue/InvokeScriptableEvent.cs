using Kurisu.NGDS;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Invoke a scriptableEvent")]
    [AkiLabel("Dialogue: Invoke ScriptableEvent")]
    [AkiGroup("Dialogue")]
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
