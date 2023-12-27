using UnityEngine;
namespace Kurisu.NGDT
{
    public abstract class Conditional : NodeBehavior, IIterable
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
        public NodeBehavior GetChildAt(int index)
        {
            return child;
        }

        public int GetChildCount()
        {
            return child == null ? 0 : 1;
        }
    }

}