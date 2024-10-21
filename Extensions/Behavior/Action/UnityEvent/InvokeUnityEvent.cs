using UnityEngine.Events;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action : Invoke a UnityEvent, should be noticed that it may loss reference during serialization ")]
    [NodeLabel("UnityEvent : Invoke UnityEvent")]
    [NodeGroup("UnityEvent")]
    public class InvokeUnityEvent : Action
    {
        [WrapField]
        public UnityEvent unityEvent;
        protected override Status OnUpdate()
        {
            unityEvent?.Invoke();
            return Status.Success;
        }
    }
}
