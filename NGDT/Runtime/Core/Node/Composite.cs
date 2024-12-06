using System.Collections.Generic;
using System.Linq;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    public abstract class Composite : NodeBehavior
    {
        [SerializeReference]
        private List<NodeBehavior> children = new();

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
        public sealed override void SetChildren(CeresNode[] inChildren)
        {
            children.Clear();
            foreach (var child in inChildren)
            {
                children.Add(child as NodeBehavior);
            }
        }
    }
}