using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Convert float type to int type")]
    [CeresLabel("Math: Float2Int")]
    [CeresGroup("Math")]
    public class Float2Int : Action
    {
        [SerializeField]
        private SharedFloat value;
        [SerializeField, ForceShared]
        private SharedInt storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = (int)value.Value;
            return Status.Success;
        }
    }
}
