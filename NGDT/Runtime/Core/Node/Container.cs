using System;
using System.Collections.Generic;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    public abstract class Container : NodeBehavior
    {
        [SerializeReference]
        private List<NodeBehavior> children = new();
        
        protected IDialogueBuilder Builder { get; private set; }
        
        public List<NodeBehavior> Children => children;
        
        protected sealed override void OnRun()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].Run(GameObject, Graph);
            }
        }

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
            children.Add(child as NodeBehavior);
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
