using System;
using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Ceres.Editor.Graph.Flow;
using Ceres.Graph.Flow;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(ExecuteFlowModule))]
    public class ExecuteFlowModuleNodeView : ModuleNodeView
    {
        public ExecuteFlowModuleNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            mainContainer.Add(new Button(EditFlowEvent) { text = "Open in Flow Graph" });
        }

        private void EditFlowEvent()
        {
            if (Graph.DialogueGraphContainer is not IFlowGraphContainer flowGraphContainer) return;
            
            var window = FlowGraphEditorWindow.Show(flowGraphContainer);
            window.SetContainerType(typeof(NextGenDialogueComponent));
            var graphView = window.GetGraphView(); 
            
            /* Container is Piece */
            var parentPiece = GetFirstAncestorOfType<PieceContainerView>();
            if (parentPiece != null)
            {
                var eventNodeView = graphView.NodeViews
                    .OfType<ExecutionEventNodeView>()
                    .FirstOrDefault(x => x.GetEventName() == $"Flow_{parentPiece.GetPieceID()}");
                if (eventNodeView == null)
                {
                    eventNodeView = new ExecutionEventNodeView(typeof(ExecutionEvent), graphView);
                    graphView.AddNodeView(eventNodeView);
                    eventNodeView.SetEventName($"Flow_{parentPiece.GetPieceID()}");
                }

                graphView.ClearSelection();
                graphView.AddToSelection(eventNodeView.NodeElement);
                graphView.schedule.Execute(() => graphView.FrameSelection()).ExecuteLater(10);
            }
            
            /* Container is Option */
            var parentOption = GetFirstAncestorOfType<OptionContainerView>();
            if (parentOption != null)
            {
                parentPiece = parentOption.GetConnectedPieceContainer();
                var options = parentPiece.GetConnectedOptionContainers();
                int index = Array.IndexOf(options, parentOption);
                string eventName = $"Flow_{parentPiece.GetPieceID()}_Option{index}";
                var eventNodeView = graphView.NodeViews
                    .OfType<ExecutionEventNodeView>()
                    .FirstOrDefault(x => x.GetEventName() == eventName);
                if (eventNodeView == null)
                {
                    eventNodeView = new ExecutionEventNodeView(typeof(ExecutionEvent), graphView);
                    graphView.AddNodeView(eventNodeView);
                    eventNodeView.SetEventName(eventName);
                }

                graphView.ClearSelection();
                graphView.AddToSelection(eventNodeView.NodeElement);
                graphView.schedule.Execute(() => graphView.FrameSelection()).ExecuteLater(10);
            }
        }
    }
}
