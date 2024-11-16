using Ceres;
using Ceres.Annotations;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set int value")]
    [NodeLabel("Math: SetInt")]
    [NodeGroup("Math")]
    public class SetInt : Action
    {
        public SharedInt intValue;
        [ForceShared, FormerlySerializedAs("intToSet")]
        public SharedInt storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = intValue.Value;
            return Status.Success;
        }
    }
}