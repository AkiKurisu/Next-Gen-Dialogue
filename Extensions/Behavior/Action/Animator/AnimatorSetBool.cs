using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set the Bool element of Animator according to status")]
    [CeresLabel("Animator: SetBool")]
    [NodeGroup("Animator")]
    public class AnimatorSetBool : AnimatorAction
    {
        public SharedString parameter;
        public SharedBool status;
        private int parameterHash;
        public override void Awake()
        {
            base.Awake();
            parameterHash = Animator.StringToHash(parameter.Value);
        }
        protected override Status OnUpdate()
        {
            Animator.SetBool(parameterHash, status.Value);
            return Status.Success;
        }
    }
}