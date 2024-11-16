using Ceres.Node;
using UnityEngine;
namespace Kurisu.NGDT
{
    public abstract class Conditional : NodeBehavior
    {

        [SerializeReference]
        private NodeBehavior child;

        public NodeBehavior Child
        {
            get => child;
#if UNITY_EDITOR
            set => child = value;
#endif
        }

        protected sealed override void OnRun()
        {
            child?.Run(GameObject, Tree);
        }

        public sealed override void Awake()
        {
            OnAwake();
            child?.Awake();
        }
        protected virtual void OnAwake()
        {
        }

        public override void Start()
        {
            OnStart();
            child?.Start();
        }

        protected virtual void OnStart()
        {
        }

        protected override Status OnUpdate()
        {
            // no child means leaf node
            if (child == null)
            {
                return IsUpdatable();
            }
            if (IsUpdatable() == Status.Success)
            {
                var status = child.Update();
                return status;
            }
            return Status.Failure;
        }

        protected abstract Status IsUpdatable();
        
        public sealed override CeresNode GetChildAt(int index)
        {
            return child;
        }
        public sealed override int GetChildrenCount()
        {
            return child == null ? 0 : 1;
        }
        public sealed override void ClearChildren()
        {
            child = null;
        }
        public sealed override void AddChild(CeresNode nodeBehavior)
        {
            child = nodeBehavior as NodeBehavior;
        }
        public sealed override void SetChildren(CeresNode[] inChildren)
        {
            child = inChildren[0] as NodeBehavior;
        }
        public sealed override CeresNode[] GetChildren()
        {
            if (child == null) return base.GetChildren();
            return new CeresNode[1] { child };
        }
    }

}