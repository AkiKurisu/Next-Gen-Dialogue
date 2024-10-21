using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Get the Bool element of the Animator")]
    [NodeLabel("Animator: GetBool")]
    [NodeGroup("Animator")]
    public class AnimatorGetBool : AnimatorAction
    {
        public SharedString parameter;
        public SharedBool storeResult;
        private int parameterHash;
        public override void Awake()
        {
            base.Awake();
            parameterHash = Animator.StringToHash(parameter.Value);
        }
        protected override Status OnUpdate()
        {
            storeResult.Value = Animator.GetBool(parameterHash);
            return Status.Success;
        }
    }
}