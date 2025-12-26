using System;
using Ceres.Annotations;

namespace NextGenDialogue.Graph
{
    [Obsolete("PreUpdateModule is no longer used, use Flow instead.")]
    [Serializable]
    [CeresLabel("PreUpdate")]
    [NodeInfo("Module: PreUpdate is used to add action for dialogue container or dialogue piece when being generated.")]
    [ModuleOf(typeof(Dialogue))]
    [ModuleOf(typeof(Piece))]
    public class PreUpdateModule : BehaviorModule
    {
        protected sealed override Status OnUpdate()
        {
            Child?.Update();
            return Status.Success;
        }
    }
}
