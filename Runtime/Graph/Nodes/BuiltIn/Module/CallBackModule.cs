using System;
using Ceres.Annotations;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Callback")]
    [NodeInfo("Module: CallBack Module is used to add callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class CallBackModule : BehaviorModule
    {
        protected sealed override Status OnUpdate()
        {
            if (Child != null)
            {
                Graph.Builder.GetNode().AddModule(new NextGenDialogue.CallBackModule(RunChild));
            }
            return Status.Success;
        }
        
        private void RunChild()
        {
            Child?.Update();
        }
    }
}
