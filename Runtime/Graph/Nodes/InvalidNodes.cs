using System;
using Ceres.Annotations;
using UnityEngine;
namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresGroup("Hidden")]
    [CeresLabel("<color=#FFE000><b>Class Missing!</b></color>")]
    [NodeInfo("The presence of this node indicates that the namespace, class name, or assembly of the behavior may be changed.")]
    internal sealed class InvalidActionNode : ActionNode
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
    [CeresGroup("Hidden")]
    [CeresLabel("<color=#FFE000><b>Class Missing!</b></color>")]
    [NodeInfo("The presence of this node indicates that the namespace, class name, or assembly of the behavior may be changed.")]
    internal sealed class InvalidCompositeNode : CompositeNode
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