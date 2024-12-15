using Ceres;
using Ceres.Annotations;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set bool value")]
    [CeresLabel("Math: SetBool")]
    [NodeGroup("Math")]
    public class SetBool : Action
    {
        public SharedBool boolValue;
        [ForceShared, FormerlySerializedAs("boolToSet")]
        public SharedBool storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = boolValue.Value;
            return Status.Success;
        }
    }
}