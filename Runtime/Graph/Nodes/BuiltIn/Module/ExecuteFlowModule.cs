using System;
using Ceres.Annotations;

namespace NextGenDialogue.Graph
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
            if (Graph.Builder.GetNode() is NextGenDialogue.Piece piece)
            {
                Graph.FlowGraph.ExecuteEvent(Component, $"Flow_{piece.ID}");
            }
            else if (Graph.Builder.GetNode() is NextGenDialogue.Option option)
            {
                var parent = Graph.Builder.GetFirstAncestorOfType<NextGenDialogue.Piece>();
                Graph.Builder.GetNode().AddModule(new NextGenDialogue.CallBackModule(() =>
                {
                    Graph.FlowGraph.ExecuteEvent(Component, $"Flow_{parent.ID}_Option{option.Index}");
                }));
            }

            return Status.Success;
        }
    }
}
