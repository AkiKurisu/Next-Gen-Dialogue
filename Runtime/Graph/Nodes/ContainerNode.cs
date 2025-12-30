using System;
using System.Collections.Generic;
using Ceres.Graph;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    public abstract class ContainerNode : DialogueNode
    {
        [SerializeReference]
        private List<DialogueNode> children = new();

        protected DialogueBuilder Builder => Graph.Builder;
        
        public List<DialogueNode> Children => children;

        public sealed override void AddChild(CeresNode child)
        {
            children.Add((DialogueNode)child);
        }
        
        public sealed override CeresNode GetChildAt(int index)
        {
            return children[index];
        }

        public sealed override int GetChildrenCount()
        {
            return children.Count;
        }
        
        public sealed override void ClearChildren()
        {
            children.Clear();
        }
    }
}
