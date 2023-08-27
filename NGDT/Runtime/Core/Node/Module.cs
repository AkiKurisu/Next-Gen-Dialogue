using System;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    public abstract class Module : NodeBehavior
    {
        protected override void OnRun() { }
    }
    [Serializable]
    public abstract class BehaviorModule : Module
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

        public override void Awake()
        {
            child?.Awake();
        }

        public override void Start()
        {
            child?.Start();
        }
    }
    [Serializable]
    public abstract class CustomModule : Module
    {
        protected sealed override void OnRun()
        {
        }
        protected sealed override Status OnUpdate()
        {
            Tree.Builder.GetNode().AddModule(GetModule());
            return Status.Success;
        }
        protected abstract IDialogueModule GetModule();
    }
}
