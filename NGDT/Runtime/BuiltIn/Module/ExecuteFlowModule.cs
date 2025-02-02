using System;
using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Module: ExecuteFlow Module is used to add actions for dialogue piece when being generated.")]
    [ModuleOf(typeof(Piece))]
    public class ExecuteFlowModule : Module
    {
        protected override Status OnUpdate()
        {
            var piece = (NGDS.Piece)Graph.Builder.GetNode();
            Graph.FlowGraph.TryExecuteEvent(Component, $"Flow_{piece.Name}");
            return Status.Success;
        }
    }
}
