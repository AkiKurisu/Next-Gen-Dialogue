using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Set trigger parameter of the Animator")]
    [CeresLabel("Animator: SetTrigger")]
    [CeresGroup("Animator")]
    public class AnimatorSetTrigger : AnimatorAction
    {
        public SharedString parameter;
        
        private int _parameterHash;
        
        public bool resetLastTrigger = true;
        
        public override void Awake()
        {
            base.Awake();
            _parameterHash = Animator.StringToHash(parameter.Value);
        }
        
        protected override Status OnUpdate()
        {
            if (resetLastTrigger) Animator.ResetTrigger(_parameterHash);
            Animator.SetTrigger(_parameterHash);
            return Status.Success;
        }
    }
}