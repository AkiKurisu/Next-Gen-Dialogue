using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action : Get Transform.Position")]
    [CeresLabel("Transform : GetPosition")]
    [CeresGroup("Transform")]
    public class TransformGetPosition : Action
    {
        [Tooltip("Target Transform, if not filled in, it will be its own Transform")]
        public SharedUObject<Transform> target;
        
        [ForceShared]
        public SharedVector3 storeResult;
        
        protected override Status OnUpdate()
        {
            if (target.Value != null) storeResult.Value = target.Value.position;
            else storeResult.Value = GameObject.transform.position;
            return Status.Success;
        }
    }
}