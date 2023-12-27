using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDT
{
    public abstract class Container : NodeBehavior, IIterable
    {
        [SerializeReference]
        private List<NodeBehavior> children = new();
        protected IDialogueBuilder Builder { get; private set; }
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
            Builder = Tree.Builder;
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

#if UNITY_EDITOR
        public void AddChild(NodeBehavior child)
        {
            children.Add(child);
        }
#endif
        public NodeBehavior GetChildAt(int index)
        {
            return children[index];
        }

        public int GetChildCount()
        {
            return children.Count;
        }
    }
}
