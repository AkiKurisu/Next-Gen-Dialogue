using UnityEngine;
namespace Kurisu.NGDT
{
    public class Decorator : NodeBehavior, IIterable
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
        public sealed override void Start()
        {
            OnStart();
            child?.Start();
        }
        protected virtual void OnStart()
        {
        }
        protected override Status OnUpdate()
        {
            var status = child.Update();
            return OnDecorate(status);
        }
        /// <summary>
        /// Decorate child return status
        /// </summary>
        /// <param name="childStatus"></param>
        /// <returns></returns>
        protected virtual Status OnDecorate(Status childStatus)
        {
            return childStatus;
        }
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
