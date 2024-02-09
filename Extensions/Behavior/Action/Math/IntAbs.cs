using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Int type takes absolute value")]
    [AkiLabel("Math: IntAbs")]
    [AkiGroup("Math")]
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
