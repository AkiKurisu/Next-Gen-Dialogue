using System.Collections.Generic;
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
                children[i].Run(GameObject, Tree);
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

#if UNITY_EDITOR
        public void AddChild(NodeBehavior child)
        {
            children.Add(child);
        }
#endif

    }
}