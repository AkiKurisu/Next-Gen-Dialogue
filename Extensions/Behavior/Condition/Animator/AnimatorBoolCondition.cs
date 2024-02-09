using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Condition: Get the Bool parameter of Animator, if it is consistent with the status, return Status.Success, otherwise return Status.Failure")]
    [AkiLabel("Animator: BoolCondition")]
    [AkiGroup("Animator")]
    public class AnimatorBoolCondition : AnimatorCondition
    {
        public SharedString parameter;
        public SharedBool status;
        [ForceShared]
        public SharedBool storeResult;
        private int parameterHash;
        protected override void OnAwake()
        {
            base.OnAwake();
            parameterHash = Animator.StringToHash(parameter.Value);
        }
        protected override Status IsUpdatable()
        {
            storeResult.Value = Animator.GetBool(parameterHash);
            return storeResult.Value == status.Value ? Status.Success : Status.Failure;
        }
    }
}