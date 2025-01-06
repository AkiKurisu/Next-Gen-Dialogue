using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class NodeConvertor
    {
        private interface IParentAdapter
        {
            void Connect(DialogueGraphView graphView, IDialogueNode nodeToConnect);
        }
        
        private class PortAdapter : IParentAdapter
        {
            private readonly Port _port;
            
            public PortAdapter(Port port)
            {
                _port = port;
            }
            
            public void Connect(DialogueGraphView graphView, IDialogueNode nodeToConnect)
            {
                var edge = PortHelper.ConnectPorts(_port, nodeToConnect.Parent);
                graphView.Add(edge);
            }
        }
        
        private class ContainerAdapter : IParentAdapter
        {
            private readonly ContainerNode _container;
            
            public ContainerAdapter(ContainerNode container)
            {
                _container = container;
            }
            
            public void Connect(DialogueGraphView graphView, IDialogueNode nodeToConnect)
            {
                if (nodeToConnect is ModuleNode moduleNode)
                    _container.AddElement(moduleNode);
                else if (_container is IContainChild childContainer)
                    childContainer.AddChildElement(nodeToConnect, graphView);
            }
        }
        
        private readonly struct EdgePair
        {
            public readonly NodeBehavior NodeBehavior;
            
            public readonly IParentAdapter Adapter;

            public EdgePair(NodeBehavior nodeBehavior, IParentAdapter adapter)
            {
                NodeBehavior = nodeBehavior;
                Adapter = adapter;
            }
        }
        
        public RootNode ConvertToNode(DialogueGraph graph, DialogueGraphView graphView, Vector2 initPos)
        {
            var stack = new Stack<EdgePair>();
            var alreadyCreateNodes = new Dictionary<NodeBehavior, IDialogueNode>();
            RootNode root = null;
            stack.Push(new EdgePair(graph.Root, null));
            while (stack.Count > 0)
            {
                // create node
                var edgePair = stack.Pop();
                if (edgePair.NodeBehavior == null)
                {
                    continue;
                }
                // Prevent duplicating instance
                if (alreadyCreateNodes.TryGetValue(edgePair.NodeBehavior, out var nodeView))
                {
                    edgePair.Adapter?.Connect(graphView, nodeView);
                    continue;
                }
                
                nodeView = DialogueNodeFactory.Get().Create(edgePair.NodeBehavior.GetType(), graphView);
                nodeView.Restore(edgePair.NodeBehavior);
                graphView.AddNodeView(nodeView);
                var rect = edgePair.NodeBehavior.NodeData.graphPosition;
                rect.position += initPos;
                nodeView.NodeElement.SetPosition(rect);
                alreadyCreateNodes.Add(edgePair.NodeBehavior, nodeView);
                
                // connect parent
                edgePair.Adapter?.Connect(graphView, nodeView);

                // seek child
                switch (edgePair.NodeBehavior)
                {
                    case Container nb:
                        {
                            var containerNode = (ContainerNode)nodeView;
                            for (var i = nb.Children.Count - 1; i >= 0; i--)
                            {
                                stack.Push(new EdgePair(nb.Children[i], new ContainerAdapter(containerNode)));
                            }
                            break;
                        }
                    case BehaviorModule nb:
                        {
                            var module = (BehaviorModuleNode)nodeView;
                            stack.Push(new EdgePair(nb.Child, new PortAdapter(module.Child)));
                            break;
                        }
                    case Composite nb:
                        {
                            var compositeNode = (CompositeNode)nodeView;
                            var addable = nb.Children.Count - compositeNode.ChildPorts.Count;
                            if (compositeNode.NoValidate && nb.Children.Count == 0)
                            {
                                compositeNode.RemoveUnnecessaryChildren();
                                break;
                            }
                            for (var i = 0; i < addable; i++)
                            {
                                compositeNode.AddChild();
                            }

                            for (var i = 0; i < nb.Children.Count; i++)
                            {
                                stack.Push(new EdgePair(nb.Children[i], new PortAdapter(compositeNode.ChildPorts[i])));
                            }
                            break;
                        }
                    case Conditional nb:
                        {
                            var conditionalNode = (ConditionalNode)nodeView;
                            stack.Push(new EdgePair(nb.Child, new PortAdapter(conditionalNode.Child)));
                            break;
                        }
                    case Decorator nb:
                        {
                            var decoratorNode = (DecoratorNode)nodeView;
                            stack.Push(new EdgePair(nb.Child, new PortAdapter(decoratorNode.Child)));
                            break;
                        }
                    case Root nb:
                        {
                            root = (RootNode)nodeView;
                            if (nb.Child != null)
                            {
                                stack.Push(new EdgePair(nb.Child, new PortAdapter(root.Child)));
                            }
                            for (var i = 0; i < nb.Children.Count; i++)
                            {
                                stack.Push(new EdgePair(nb.Children[i], null));
                            }
                            break;
                        }
                }
            }
            return root;
        }
    }
}
