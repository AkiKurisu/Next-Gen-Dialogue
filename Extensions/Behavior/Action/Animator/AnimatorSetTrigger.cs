using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set trigger parameter of the Animator")]
    [CeresLabel("Animator: SetTrigger")]
    [NodeGroup("Animator")]
    public class AnimatorSetTrigger : AnimatorAction
    {
        public SharedString parameter;
        private int parameterHash;
        public bool resetLastTrigger = true;
        public override void Awake()
        {
            base.Awake();
            parameterHash = Animator.StringToHash(parameter.Value);
        }
        protected override Status OnUpdate()
        {
            if (resetLastTrigger) Animator.ResetTrigger(parameterHash);
            Animator.SetTrigger(parameterHash);
            return Status.Success;
        }
    }
}