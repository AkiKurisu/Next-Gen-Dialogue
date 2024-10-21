using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Log some text")]
    [NodeLabel("Debug: Log")]
    [NodeGroup("Debug")]
    public class DebugLog : Action
    {
        public SharedString logText;
        protected override Status OnUpdate()
        {
            Debug.Log(logText.Value, GameObject);
            return Status.Success;
        }
    }
}
