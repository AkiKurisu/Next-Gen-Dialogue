using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph.Flow;
using Ceres.Graph.Flow;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(ExecuteFlowModule))]
    public class ExecuteFlowModuleNode : ModuleNode
    {
        public ExecuteFlowModuleNode()
        {
            mainContainer.Add(new Button(EditFlowEvent) { text = "Open in Flow Graph" });
        }

        private void EditFlowEvent()
        {
            if (GraphView.DialogueGraphContainer is not IFlowGraphContainer flowGraphContainer) return;
            
            var window = FlowGraphEditorWindow.EditorWindowRegistry.GetOrCreateEditorWindow(flowGraphContainer);
            window.Show();
            window.Focus();
            var graphView = window.GetGraphView(); 
            var container = GetFirstAncestorOfType<PieceContainer>();
            if (container == null) return;
            var eventNodeView =  graphView.NodeViews
                .OfType<ExecutionEventNodeView>()
                .FirstOrDefault(x => x.GetEventName() == $"Flow_{container.GetPieceID()}");
            if (eventNodeView == null)
            {
                eventNodeView = new ExecutionEventNodeView(typeof(ExecutionEvent), graphView);
                graphView.AddNodeView(eventNodeView);
                eventNodeView.SetEventName( $"Flow_{container.GetPieceID()}");
            }
            graphView.ClearSelection();
            graphView.AddToSelection(eventNodeView.NodeElement);
            graphView.FrameSelection();
        }
    }
}
