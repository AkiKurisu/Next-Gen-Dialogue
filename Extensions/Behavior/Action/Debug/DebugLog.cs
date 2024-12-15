using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Log some text")]
    [CeresLabel("Debug: Log")]
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
