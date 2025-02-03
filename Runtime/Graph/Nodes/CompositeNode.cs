using System;
using System.Collections.Generic;
using Ceres.Graph;
using UnityEngine;
namespace NextGenDialogue.Graph
{
    [Serializable]
    public abstract class CompositeNode : DialogueNode
    {
        [SerializeReference]
        private List<DialogueNode> children = new();

        public List<DialogueNode> Children => children;

        public sealed override void Awake()
        {
            OnAwake();
            foreach (var node in children)
            {
                node.Awake();
            }
        }

        protected virtual void OnAwake()
        {
        }

        public sealed override void Start()
        {
            OnStart();
            foreach (var node in children)
            {
                node.Start();
            }
        }

        protected virtual void OnStart()
        {
        }

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