using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class NodeConvertor
    {
        private interface IParentAdapter
        {
            void Connect(IDialogueTreeView treeView, IDialogueNode nodeToConnect);
        }
        private readonly struct PortAdapter : IParentAdapter
        {
            private readonly Port port;
            public PortAdapter(Port port)
            {
                this.port = port;
            }
            public void Connect(IDialogueTreeView treeView, IDialogueNode nodeToConnect)
            {
                var edge = PortHelper.ConnectPorts(port, nodeToConnect.Parent);
                treeView.View.Add(edge);
            }
        }
        private readonly struct ContainerAdapter : IParentAdapter
        {
            private readonly ContainerNode container;
            public ContainerAdapter(ContainerNode container)
            {
                this.container = container;
            }
            public void Connect(IDialogueTreeView treeView, IDialogueNode nodeToConnect)
            {
                if (nodeToConnect is ModuleNode moduleNode)
                    container.AddElement(moduleNode);
                else if (container is IContainChild childContainer)
                    childContainer.AddChildElement(nodeToConnect, treeView);
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
        private readonly NodeResolverFactory nodeResolver = NodeResolverFactory.Instance;
        private readonly List<IDialogueNode> tempNodes = new();
        public (RootNode, IEnumerable<IDialogueNode>) ConvertToNode(IDialogueTree tree, IDialogueTreeView treeView, Vector2 initPos)
        {
            var stack = new Stack<EdgePair>();
            var alreadyCreateNodes = new Dictionary<NodeBehavior, IDialogueNode>();
            RootNode root = null;
            stack.Push(new EdgePair(tree.Root, null));
            tempNodes.Clear();
            while (stack.Count > 0)
            {
                // create node
                var edgePair = stack.Pop();
                if (edgePair.NodeBehavior == null)
                {
                    continue;
                }
                //Prevent duplicating instance
                if (alreadyCreateNodes.ContainsKey(edgePair.NodeBehavior))
                {
                    edgePair.Adapter?.Connect(treeView, alreadyCreateNodes[edgePair.NodeBehavior]);
                    continue;
                }
                IDialogueNode node = nodeResolver.Create(edgePair.NodeBehavior.GetType(), treeView);
                node.Restore(edgePair.NodeBehavior);
                treeView.View.AddElement(node as Node);
                tempNodes.Add(node);
                var rect = edgePair.NodeBehavior.graphPosition;
                rect.position += initPos;
                (node as Node).SetPosition(rect);
                alreadyCreateNodes.Add(edgePair.NodeBehavior, node);
                // connect parent
                edgePair.Adapter?.Connect(treeView, node);

                // seek child
                switch (edgePair.NodeBehavior)
                {
                    case Container nb:
                        {
                            var containerNode = node as ContainerNode;
                            for (var i = nb.Children.Count - 1; i >= 0; i--)
                            {
                                stack.Push(new EdgePair(nb.Children[i], new ContainerAdapter(containerNode)));
                            }
                            break;
                        }
                    case BehaviorModule nb:
                        {
                            var module = node as BehaviorModuleNode;
                            stack.Push(new EdgePair(nb.Child, new PortAdapter(module.Child)));
                            break;
                        }
                    case Composite nb:
                        {
                            var compositeNode = node as CompositeNode;
                            var addible = nb.Children.Count - compositeNode.ChildPorts.Count;
                            for (var i = 0; i < addible; i++)
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
                            var conditionalNode = node as ConditionalNode;
                            stack.Push(new EdgePair(nb.Child, new PortAdapter(conditionalNode.Child)));
                            break;
                        }
                    case Decorator nb:
                        {
                            var decoratorNode = node as DecoratorNode;
                            stack.Push(new EdgePair(nb.Child, new PortAdapter(decoratorNode.Child)));
                            break;
                        }
                    case Root nb:
                        {
                            root = node as RootNode;
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
            return (root, tempNodes);
        }
    }
}
