using System;

namespace NextGenDialogue.Graph
{
    [Serializable]
    public abstract class Module : DialogueNode
    {
        
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
