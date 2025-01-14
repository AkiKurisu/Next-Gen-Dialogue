using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Set the Bool element of Animator according to status")]
    [CeresLabel("Animator: SetBool")]
    [CeresGroup("Animator")]
    public class AnimatorSetBool : AnimatorAction
    {
        public SharedString parameter;
        
        public SharedBool status;
        
        private int _parameterHash;
        
        public override void Awake()
        {
            base.Awake();
            _parameterHash = Animator.StringToHash(parameter.Value);
        }
        
        protected override Status OnUpdate()
        {
            Animator.SetBool(_parameterHash, status.Value);
            return Status.Success;
        }
    }
}