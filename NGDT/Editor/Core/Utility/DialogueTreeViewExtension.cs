using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueTreeViewExtension
    {
        public static T GetSharedVariableValue<T>(this IDialogueTreeView treeView, SharedVariable<T> variable)
        {
            if (variable.IsShared)
            {
                if (!treeView.TryGetProperty(variable.Name, out SharedVariable<T> mapContent)) return variable.Value;
                return mapContent.Value;
            }
            else
            {
                return variable.Value;
            }
        }
        public static bool TryGetProperty<T>(this IDialogueTreeView treeView, string name, out T variable) where T : SharedVariable
        {
            variable = (T)treeView.SharedVariables.Where(x => x is T && x.Name.Equals(name)).FirstOrDefault();
            return variable != null;
        }
        public static PieceContainer FindPiece(this IDialogueTreeView treeView, string pieceID)
        {
            return treeView.View.nodes.OfType<PieceContainer>().FirstOrDefault(x => x.GetPieceID() == pieceID);
        }
        public static List<T> CollectNodes<T>(this IDialogueTreeView treeView) where T : Node
        {
            return treeView.View.nodes.OfType<T>().ToList();
        }
        /// <summary>
        /// Create an empty node and add to tree view
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="position"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDialogueNode CreateNode<T>(this IDialogueTreeView treeView, Vector2 position) where T : NodeBehavior
        {
            var node = NodeResolverFactory.Instance.Create(typeof(T), treeView);
            treeView.AddNode(node, new(0, 0, position.x, position.y));
            return node;
        }
        /// <summary>
        /// Create a node from behavior and add to tree view
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="behavior"></param>
        /// <param name="position"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IDialogueNode CreateNode<T>(this IDialogueTreeView treeView, T behavior, Vector2 position) where T : NodeBehavior
        {
            var node = NodeResolverFactory.Instance.Create(typeof(T), treeView);
            node.Restore(behavior);
            treeView.AddNode(node, new(0, 0, position.x, position.y));
            return node;
        }
        public static ContainerNode CreateNextContainer(this IDialogueTreeView treeView, ContainerNode first)
        {
            Type nextNodeType = first is PieceContainer ? typeof(Option) : typeof(Piece);
            var node = NodeResolverFactory.Instance.Create(nextNodeType, treeView) as ContainerNode;
            var rect = first.GetPosition();
            rect.x += rect.width + 300;
            treeView.AddNode(node, rect);
            return node;
        }
        /// <summary>
        /// Get cancellation token source attached to tree view for preventing threads from running in the background
        /// when tree view is already detached
        /// </summary>
        /// <param name="treeView"></param>
        /// <returns></returns>
        public static CancellationTokenSource GetCancellationTokenSource(this IDialogueTreeView treeView)
        {
            var ct = new CancellationTokenSource();
            treeView.View.RegisterCallback<DetachFromPanelEvent>((evt) => ct.Cancel());
            return ct;
        }
        public static void ConnectContainerNodes(this IDialogueTreeView treeView, ContainerNode first, ContainerNode second)
        {
            if (first is PieceContainer pieceContainer)
            {
                pieceContainer.AddChildElement(second, treeView);
            }
            else
            {
                var optionNode = first as OptionContainer;
                var pieceNode = second as PieceContainer;
                pieceNode.GenerateNewPieceID();
                optionNode.AddModuleNode(new TargetIDModule(pieceNode.GetPieceID()));
            }
        }
    }
}
