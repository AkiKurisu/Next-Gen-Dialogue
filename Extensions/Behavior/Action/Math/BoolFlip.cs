using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Flip bool value")]
    [AkiLabel("Math: BoolFlip")]
    [AkiGroup("Math")]
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