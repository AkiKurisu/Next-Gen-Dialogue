using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Get the Bool element of the Animator")]
    [CeresLabel("Animator: GetBool")]
    [NodeGroup("Animator")]
    public class AnimatorGetBool : AnimatorAction
    {
        public SharedString parameter;
        
        public SharedBool storeResult;
        
        private int _parameterHash;
        
        public override void Awake()
        {
            base.Awake();
            _parameterHash = Animator.StringToHash(parameter.Value);
        }
        
        protected override Status OnUpdate()
        {
            storeResult.Value = Animator.GetBool(_parameterHash);
            return Status.Success;
        }
    }
}