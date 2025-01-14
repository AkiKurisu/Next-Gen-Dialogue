using System;
using Ceres.Annotations;
using UnityEngine.Events;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action : Invoke a UnityEvent, should be noticed that it may loss reference during serialization ")]
    [CeresLabel("UnityEvent : Invoke UnityEvent")]
    [CeresGroup("UnityEvent")]
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
