using System;
using System.Collections.Generic;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    public abstract class Container : DialogueNode
    {
        [SerializeReference]
        private List<DialogueNode> children = new();
        
        protected IDialogueBuilder Builder { get; private set; }
        
        public List<DialogueNode> Children => children;

        public sealed override void Awake()
        {
            Builder = Graph.Builder;
            OnAwake();
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Awake();
            }
        }

        protected virtual void OnAwake()
        {
        }

        public sealed override void Start()
        {
            OnStart();
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Start();
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
            children.Add(child as DialogueNode);
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
