using System;
using System.Collections.Generic;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    public abstract class ContainerNode : DialogueNode
    {
        [SerializeReference]
        private List<DialogueNode> children = new();
        
        protected DialogueBuilder Builder { get; private set; }
        
        public List<DialogueNode> Children => children;

        public sealed override void Awake()
        {
            Builder = Graph.Builder;
            OnAwake();
            foreach (var childNode in children)
            {
                childNode.Awake();
            }
        }

        protected virtual void OnAwake()
        {
        }

        public sealed override void Start()
        {
            OnStart();
            foreach (var childNode in children)
            {
                childNode.Start();
            }
        }
        
        public virtual void Abort()
        {

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
