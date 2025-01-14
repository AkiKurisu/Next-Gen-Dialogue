using Ceres;
using Ceres.Annotations;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set int value")]
    [CeresLabel("Math: SetInt")]
    [CeresGroup("Math")]
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