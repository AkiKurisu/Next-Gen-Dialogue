using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace NextGenDialogue.Graph.Editor
{
    public static class DialogueGraphViewExtension
    {
        public static PieceContainerView FindPiece(this DialogueGraphView graphView, string pieceID)
        {
            return graphView.nodes.OfType<PieceContainerView>().FirstOrDefault(x => x.GetPieceID() == pieceID);
        }
        
        public static List<T> CollectNodes<T>(this DialogueGraphView graphView) where T : UnityEditor.Experimental.GraphView.Node
        {
            return graphView.nodes.OfType<T>().ToList();
        }
        
        /// <summary>
        /// Create an empty node and add to tree view
        /// </summary>
        /// <param name="graphView"></param>
        /// <param name="position"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDialogueNodeView CreateNode<T>(this DialogueGraphView graphView, Vector2 position) where T : DialogueNode
        {
            var node = (IDialogueNodeView)NodeViewFactory.Get().CreateInstance(typeof(T), graphView);
            graphView.AddNodeView(node, new Rect(0, 0, position.x, position.y));
            return node;
        }
        
        /// <summary>
        /// Create a node from behavior and add to tree view
        /// </summary>
        /// <param name="graphView"></param>
        /// <param name="behavior"></param>
        /// <param name="position"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDialogueNodeView CreateNode<T>(this DialogueGraphView graphView, T behavior, Vector2 position) where T : DialogueNode
        {
            var node = (IDialogueNodeView)NodeViewFactory.Get().CreateInstance(typeof(T), graphView);
            node.SetNodeInstance(behavior);
            graphView.AddNodeView(node, new Rect(0, 0, position.x, position.y));
            return node;
        }
        
        public static ContainerNodeView CreateNextContainer(this DialogueGraphView graphView, ContainerNodeView first)
        {
            var nextNodeType = first is PieceContainerView ? typeof(Option) : typeof(Piece);
            var node = (ContainerNodeView)NodeViewFactory.Get().CreateInstance(nextNodeType, graphView);
            var rect = first.GetPosition();
            rect.x += rect.width + 300;
            graphView.AddNodeView(node, rect);
            return node;
        }
        
        /// <summary>
        /// Get cancellation token source attached to tree view for preventing threads from running in the background
        /// when tree view is already detached
        /// </summary>
        /// <param name="graphView"></param>
        /// <returns></returns>
        public static CancellationTokenSource GetCancellationTokenSource(this DialogueGraphView graphView)
        {
            var ct = new CancellationTokenSource();
            graphView.RegisterCallback<DetachFromPanelEvent>(_ => ct.Cancel());
            return ct;
        }
        
        public static void ConnectContainerNodes(this DialogueGraphView graphView, ContainerNodeView first, ContainerNodeView second)
        {
            if (first is PieceContainerView pieceContainer)
            {
                pieceContainer.AddChildElement(second, graphView);
            }
            else
            {
                var optionNode = (OptionContainerView)first;
                var pieceNode = (PieceContainerView)second;
                pieceNode.GenerateNewPieceID();
                optionNode.AddModuleNode(new TargetIDModule(pieceNode.GetPieceID()));
            }
        }
    }
}
