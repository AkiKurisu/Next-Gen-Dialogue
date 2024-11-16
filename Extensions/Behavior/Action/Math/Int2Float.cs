using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Convert int type to float type")]
    [NodeLabel("Math: Int2Float")]
    [NodeGroup("Math")]
    public class Int2Float : Action
    {
        [SerializeField]
        private SharedInt value;
        [SerializeField, ForceShared]
        private SharedFloat newValue;
        protected override Status OnUpdate()
        {
            newValue.Value = (float)value.Value;
            return Status.Success;
        }
    }
}
