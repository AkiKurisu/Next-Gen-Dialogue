using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueGraphViewExtension
    {
        public static PieceContainer FindPiece(this DialogueGraphView graphView, string pieceID)
        {
            return graphView.nodes.OfType<PieceContainer>().FirstOrDefault(x => x.GetPieceID() == pieceID);
        }
        
        public static List<T> CollectNodes<T>(this DialogueGraphView graphView) where T : Node
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
        public static IDialogueNode CreateNode<T>(this DialogueGraphView graphView, Vector2 position) where T : NodeBehavior
        {
            var node = DialogueNodeFactory.Get().Create(typeof(T), graphView);
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
        public static IDialogueNode CreateNode<T>(this DialogueGraphView graphView, T behavior, Vector2 position) where T : NodeBehavior
        {
            var node = DialogueNodeFactory.Get().Create(typeof(T), graphView);
            node.Restore(behavior);
            graphView.AddNodeView(node, new Rect(0, 0, position.x, position.y));
            return node;
        }
        public static ContainerNode CreateNextContainer(this DialogueGraphView graphView, ContainerNode first)
        {
            Type nextNodeType = first is PieceContainer ? typeof(Option) : typeof(Piece);
            var node = DialogueNodeFactory.Get().Create(nextNodeType, graphView) as ContainerNode;
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
            graphView.RegisterCallback<DetachFromPanelEvent>((evt) => ct.Cancel());
            return ct;
        }
        public static void ConnectContainerNodes(this DialogueGraphView graphView, ContainerNode first, ContainerNode second)
        {
            if (first is PieceContainer pieceContainer)
            {
                pieceContainer.AddChildElement(second, graphView);
            }
            else
            {
                var optionNode = (OptionContainer)first;
                var pieceNode = (PieceContainer)second;
                pieceNode.GenerateNewPieceID();
                optionNode.AddModuleNode(new TargetIDModule(pieceNode.GetPieceID()));
            }
        }
    }
}
