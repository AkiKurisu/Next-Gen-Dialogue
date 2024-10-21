using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Int type takes absolute value")]
    [NodeLabel("Math: IntAbs")]
    [NodeGroup("Math")]
    public class IntAbs : Action
    {
        [SerializeField, ForceShared]
        private SharedInt value;
        protected override Status OnUpdate()
        {
            value.Value = Mathf.Abs(value.Value);
            return Status.Success;
        }
    }
}
