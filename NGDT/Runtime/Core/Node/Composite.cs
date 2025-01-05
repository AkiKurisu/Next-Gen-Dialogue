using System;
using System.Collections.Generic;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    public abstract class Composite : NodeBehavior
    {
        [SerializeReference]
        private List<NodeBehavior> children = new();

        public List<NodeBehavior> Children => children;

        protected sealed override void OnRun()
        {
            foreach (var node in children)
            {
                node.Run(GameObject, Graph);
            }
        }

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
            children.Add((NodeBehavior)child);
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