using System;
using Ceres.Graph;
using UnityEngine;
namespace NextGenDialogue.Graph
{
    [Serializable]
    public abstract class Module : DialogueNode
    {
        
    }
    
    [Serializable]
    public abstract class BehaviorModule : Module
    {
        [SerializeReference]
        private DialogueNode child;
        
        public DialogueNode Child
        {
            get => child;
            set => child = value;
        }

        public override void Awake()
        {
            child?.Awake();
        }

        public override void Start()
        {
            child?.Start();
        }
        
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
            child = nodeBehavior as DialogueNode;
        }
    }
    
    [Serializable]
    public abstract class CustomModule : Module
    {
        protected sealed override Status OnUpdate()
        {
            Graph.Builder.GetNode().AddModule(GetModule());
            return Status.Success;
        }
        
        protected abstract IDialogueModule GetModule();
    }
}
