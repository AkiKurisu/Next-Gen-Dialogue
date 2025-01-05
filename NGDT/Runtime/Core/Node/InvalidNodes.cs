using System;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeGroup("Hidden")]
    [CeresLabel("<color=#FFE000><b>Class Missing!</b></color>")]
    [NodeInfo("The presence of this node indicates that the namespace, class name, or assembly of the behavior may be changed.")]
    internal sealed class InvalidAction : Action
    {
        [Multiline]
        public string nodeType;
        
        [Multiline]
        public string serializedData;
        
        protected override Status OnUpdate()
        {
            return Status.Success;
        }
    }
    
    [Serializable]
    [NodeGroup("Hidden")]
    [CeresLabel("<color=#FFE000><b>Class Missing!</b></color>")]
    [NodeInfo("The presence of this node indicates that the namespace, class name, or assembly of the behavior may be changed.")]
    internal sealed class InvalidComposite : Composite
    {
        [Multiline]
        public string nodeType;
        
        [Multiline]
        public string serializedData;
        
        protected override Status OnUpdate()
        {
            return Status.Success;
        }
    }
}