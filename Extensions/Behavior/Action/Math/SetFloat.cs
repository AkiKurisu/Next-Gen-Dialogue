using Ceres;
using Ceres.Annotations;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set float value")]
    [CeresLabel("Math: SetFloat")]
    [NodeGroup("Math")]
    public class SetFloat : Action
    {
        public SharedFloat floatValue;
        [ForceShared, FormerlySerializedAs("floatToSet")]
        public SharedFloat storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = floatValue.Value;
            return Status.Success;
        }
    }
}