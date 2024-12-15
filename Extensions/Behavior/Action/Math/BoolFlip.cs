using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Flip bool value")]
    [CeresLabel("Math: BoolFlip")]
    [NodeGroup("Math")]
    public class BoolFlip : Action
    {
        [SerializeField, ForceShared]
        private SharedBool boolToFlip;
        protected override Status OnUpdate()
        {
            boolToFlip.Value = !boolToFlip.Value;
            return Status.Success;
        }
    }
}