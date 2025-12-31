using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Graph;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    public interface IDialogueGraphSerializeVisitor
    {
        void Visit(ContainerNodeView containerNodeView, ContainerNode instance);
    }
    
    public partial class DialogueGraphView
    {
        private class SerializeVisitor: IDialogueGraphSerializeVisitor
        {
            private readonly Stack<IDialogueNodeView> _stack = new();

            private List<IDialogueNodeView> _selectedNodes;

            private readonly Dictionary<IDialogueNodeView, DialogueNode> _instances = new();

            private DialogueGraphView GraphView { get; }

            private Root Root { get; }

            public SerializeVisitor(DialogueGraphView dialogueGraphView, Root root, List<IDialogueNodeView> selectedNodes = null)
            {
                GraphView = dialogueGraphView;
                Root = root;
                _selectedNodes = selectedNodes;
            }

            private bool CanSerialize(IDialogueNodeView nodeView)
            {
                return _selectedNodes == null || _selectedNodes.Contains(nodeView);
            }

            private DialogueNode CompileNode(IDialogueNodeView nodeView)
            {
                if (!_instances.TryGetValue(nodeView, out var node))
                {
                    _instances.Add(nodeView, node = (DialogueNode)Activator.CreateInstance(nodeView.NodeType));
                }

                return node;
            }

            /// <summary>
            /// Serialize from providing node views
            /// </summary>
            /// <param name="nodeViews"></param>
            /// <returns></returns>
            public DialogueGraph Serialize(List<IDialogueNodeView> nodeViews)
            {
                _selectedNodes = new List<IDialogueNodeView>(nodeViews);
                
                // Commit all modules
                nodeViews.OfType<ModuleNodeView>().ToList().ForEach(view =>
                {
                    var parentView = view.GetFirstAncestorOfType<ContainerNodeView>();
                    var parentNode = parentView == null ? Root : CanSerialize(parentView) ? CompileNode(parentView) : Root;
                    parentNode.AddChild(CompileNode(view));
                    _stack.Push(view);
                });
                
                // Commit all containers
                nodeViews.OfType<ContainerNodeView>().ToList()
                    .ForEach(view =>
                    {
                        Root.AddChild(CompileNode(view));
                        _stack.Push(view);
                    });

                while (_stack.Count > 0)
                {
                    var nodeView = _stack.Pop();
                    nodeView.SerializeNode(this, CompileNode(nodeView));
                }

                return Build_Internal();
            }

            /// <summary>
            /// Serialize from root view
            /// </summary>
            /// <param name="rootNodeView"></param>
            /// <returns></returns>
            public DialogueGraph Serialize(RootNodeView rootNodeView)
            {
                var containerNodes = GraphView.CollectNodes<ContainerNodeView>();

                // Commit active dialogue first
                if (rootNodeView.Child.connected)
                {
                    var child = (DialogueContainerView)PortHelper.FindChildNode(rootNodeView.Child);
                    Root.AddChild(CompileNode(child));
                    _stack.Push(child);
                    containerNodes.Remove(child);
                }
                else
                {
                    // Add empty dialogue
                    Root.AddChild(new Dialogue());
                }
                
                // Commit all containers
                containerNodes.ForEach(containerView =>
                {
                    Root.AddChild(CompileNode(containerView));
                    _stack.Push(containerView);
                });

                // Commit all modules
                GraphView.CollectNodes<ModuleNodeView>().ToList().ForEach(view =>
                {
                    var parentView = view.GetFirstAncestorOfType<ContainerNodeView>();
                    var parentNode = parentView == null ? Root : CompileNode(parentView);
                    parentNode.AddChild(CompileNode(view));
                    _stack.Push(view);
                });
                
                // Commit node instances
                while (_stack.Count > 0)
                {
                    var nodeView = _stack.Pop();
                    nodeView.SerializeNode(this, CompileNode(nodeView));
                }

                return Build_Internal();
            }

            /// <summary>
            /// Visitor container node view and commit to target instance
            /// </summary>
            /// <param name="containerNodeView"></param>
            /// <param name="instance"></param>
            public void Visit(ContainerNodeView containerNodeView, ContainerNode instance)
            {
                if (!CanSerialize(containerNodeView)) return;
                
                // Commit bridge node
                var bridges = containerNodeView.contentContainer.Query<ChildBridgeView>().ToList();
                bridges.ForEach(bridge =>
                {
                    if (bridge is PieceBridgeView pieceBridgeView)
                    {
                        Visit_Internal(pieceBridgeView, (Dialogue)instance);
                    }
                    else if (bridge is OptionBridgeView optionBridgeView)
                    {
                        Visit_Internal(optionBridgeView, instance);
                    }
                });
            }

            private void Visit_Internal(PieceBridgeView bridgeView, Dialogue instance)
            {
                if (bridgeView.UseReference)
                {
                    var pieceView = GraphView.FindPiece(bridgeView.PieceID);
                    if (!CanSerialize(pieceView)) return;
                    if (pieceView == null) return;

                    instance.AddPiece(pieceView.GetPiece(), bridgeView.PieceID);
                }
                else if (bridgeView.Child.connected)
                {
                    var pieceView = (PieceContainerView)PortHelper.FindChildNode(bridgeView.Child);
                    if (!CanSerialize(pieceView)) return;

                    instance.AddPiece(pieceView.GetPiece(), string.Empty);
                }
            }

            private void Visit_Internal(OptionBridgeView bridgeView, ContainerNode instance)
            {
                if (bridgeView.Child.connected)
                {
                    var node = PortHelper.FindChildNode(bridgeView.Child);
                    if (!CanSerialize(node)) return;

                    _stack.Push(node);
                    instance.AddChild(CompileNode(node));
                }
            }

            private DialogueGraph Build_Internal()
            {
                var graph = new DialogueGraph();
                graph.nodes = new List<CeresNode> { Root };
                graph.nodes.AddRange(Root);
                return graph;
            }
        }
    }
}