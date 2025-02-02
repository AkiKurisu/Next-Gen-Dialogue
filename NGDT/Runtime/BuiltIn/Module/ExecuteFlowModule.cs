using System;
using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [Serializable]
    [CeresLabel("Execute Flow")]
    [NodeInfo("Module: Execute Flow is used to add custom actions for piece and option.")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class ExecuteFlowModule : Module
    {
        protected override Status OnUpdate()
        {
            if (Graph.Builder.GetNode() is NGDS.Piece piece)
            {
                Graph.FlowGraph.TryExecuteEvent(Component, $"Flow_{piece.Name}");
            }
            else if (Graph.Builder.GetNode() is NGDS.Option option)
            {
                var parent = Graph.Builder.GetFirstAncestorOfType<NGDS.Piece>();
                Graph.Builder.GetNode().AddModule(new NGDS.CallBackModule(() =>
                {
                    Graph.FlowGraph.TryExecuteEvent(Component, $"Flow_{parent.Name}_Option{option.Index}");
                }));
            }

            return Status.Success;
        }
    }
}
