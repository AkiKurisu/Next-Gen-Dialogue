using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Condition: Get the Bool parameter of Animator, if it is consistent with the status, return Status.Success, otherwise return Status.Failure")]
    [CeresLabel("Animator: BoolCondition")]
    [CeresGroup("Animator")]
    public class AnimatorBoolCondition : AnimatorCondition
    {
        public SharedString parameter;
        
        public SharedBool status;
        
        [ForceShared]
        public SharedBool storeResult;
        
        private int _parameterHash;
        
        protected override void OnAwake()
        {
            base.OnAwake();
            _parameterHash = Animator.StringToHash(parameter.Value);
        }
        
        protected override Status IsUpdatable()
        {
            storeResult.Value = Animator.GetBool(_parameterHash);
            return storeResult.Value == status.Value ? Status.Success : Status.Failure;
        }
    }
}