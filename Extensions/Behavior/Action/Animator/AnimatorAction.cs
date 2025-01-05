using System;
using Ceres;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    public abstract class AnimatorAction : Action
    {
        [Tooltip("If not filled in, it will be obtained from the bound gameObject")]
        public SharedUObject<Animator> animator;
        
        protected Animator Animator => animator.Value;
        
        public override void Awake()
        {
            if (animator.Value == null) animator.Value = GameObject.GetComponent<Animator>();
        }
    }
}