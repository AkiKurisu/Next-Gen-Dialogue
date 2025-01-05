using System;
using Ceres;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    public abstract class AnimatorCondition : Conditional
    {
        [Tooltip("If not filled in, it will be obtained from the bound gameObject")]
        public SharedUObject<Animator> animator;
        
        protected Animator Animator => animator.Value;
        
        protected override void OnAwake()
        {
            if (animator.Value == null) animator.Value = GameObject.GetComponent<Animator>();
        }
    }
}