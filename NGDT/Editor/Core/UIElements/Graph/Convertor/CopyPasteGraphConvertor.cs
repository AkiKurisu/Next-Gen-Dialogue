using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class CopyPasteGraphConvertor
    {
        private readonly DialogueTreeView sourceView;
        private readonly List<ISelectable> copyElements;
        private readonly Dictionary<Port, Port> portCopyDict;
        private readonly Dictionary<IDialogueNode, IDialogueNode> nodeCopyDict;
        private readonly List<GraphElement> sourceElements;
        private readonly HashSet<Edge> sourceEdges;
        public CopyPasteGraphConvertor(DialogueTreeView sourceView, List<GraphElement> sourceElements, Vector2 positionOffSet)
        {
            this.sourceView = sourceView;
            this.sourceElements = sourceElements;
            copyElements = new List<ISelectable>();
            portCopyDict = new Dictionary<Port, Port>();
            nodeCopyDict = new Dictionary<IDialogueNode, IDialogueNode>();
            sourceEdges = sourceElements.OfType<Edge>().ToHashSet();
            DistinctNodes();
            CopyNodes();
            CopyEdges();
            CopyGroupBlocks();
            foreach (var pair in nodeCopyDict)
            {
                Rect newRect = pair.Key.GetWorldPosition();
                newRect.position += positionOffSet;
                pair.Value.NodeElement.SetPosition(newRect);
            }
        }
        public List<ISelectable> GetCopyElements() => copyElements;
        private void DistinctNodes()
        {
            var containerNodes = sourceElements.OfType<ContainerNode>().ToArray();
            foreach (var containerNode in containerNodes)
            {
                containerNode.contentContainer.Query<ModuleNode>().ForEach(x => sourceElements.Remove(x));
            }
        }
        private void CopyNodes()
        {
            foreach (var selectNode in sourceElements.OfType<IDialogueNode>())
            {
                var node = sourceView.DuplicateNode(selectNode);
                copyElements.Add(node as Node);
                nodeCopyDict.Add(selectNode, node);
                CopyPort(selectNode, node);
            }
        }
        private void CopyPort(IDialogueNode sourceNode, IDialogueNode pasteNode)
        {
            var behaviorType = sourceNode.GetBehavior();
            if (behaviorType.IsSubclassOf(typeof(Container)))
            {
                var sourceContainer = sourceNode as ContainerNode;
                var pasteContainer = pasteNode as ContainerNode;
                var copyMap = pasteContainer.GetCopyMap();
                sourceContainer.contentContainer.Query<Node>()
                .ToList()
                .ForEach(x =>
                {
                    if (x is BehaviorModuleNode behaviorModuleNode)
                        portCopyDict.Add(behaviorModuleNode.Child, (copyMap[x.GetHashCode()] as BehaviorModuleNode).Child);
                    else if (x is ChildBridge childBridge)
                        portCopyDict.Add(childBridge.Child, (copyMap[x.GetHashCode()] as ChildBridge).Child);
                });
                //For some reason edges connected to bridge's ports are not selected by graph view
                //Add edge manually
                portCopyDict.Add(sourceContainer.Parent, pasteContainer.Parent);
                if (sourceContainer.Parent.connected)
                {
                    var edge = sourceContainer.Parent.connections.FirstOrDefault();
                    sourceEdges.Add(edge);
                }
            }
            else if (behaviorType.IsSubclassOf(typeof(BehaviorModule)))
            {
                var behaviorModuleNode = sourceNode as BehaviorModuleNode;
                portCopyDict.Add(behaviorModuleNode.Child, (pasteNode as BehaviorModuleNode).Child);
            }
            else if (behaviorType.IsSubclassOf(typeof(Action)))
            {
                var actionNode = sourceNode as ActionNode;
                portCopyDict.Add(actionNode.Parent, (pasteNode as ActionNode).Parent);
            }
            else if (behaviorType.IsSubclassOf(typeof(Composite)))
            {
                var compositeNode = sourceNode as CompositeNode;
                var copy = pasteNode as CompositeNode;
                int count = compositeNode.ChildPorts.Count - copy.ChildPorts.Count;
                for (int i = 0; i < count; i++)
                {
                    copy.AddChild();
                }
                for (int i = 0; i < compositeNode.ChildPorts.Count; i++)
                {
                    portCopyDict.Add(compositeNode.ChildPorts[i], copy.ChildPorts[i]);
                }
                portCopyDict.Add(compositeNode.Parent, copy.Parent);
            }
            else if (behaviorType.IsSubclassOf(typeof(Conditional)))
            {
                var conditionalNode = sourceNode as ConditionalNode;
                portCopyDict.Add(conditionalNode.Child, (pasteNode as ConditionalNode).Child);
                portCopyDict.Add(conditionalNode.Parent, (pasteNode as ConditionalNode).Parent);

            }
            else if (behaviorType.IsSubclassOf(typeof(Decorator)))
            {
                var decoratorNode = pasteNode as DecoratorNode;
                portCopyDict.Add(decoratorNode.Child, (pasteNode as DecoratorNode).Child);
                portCopyDict.Add(decoratorNode.Parent, (pasteNode as DecoratorNode).Parent);
            }
        }
        private void CopyEdges()
        {
            foreach (var edge in sourceEdges)
            {
                if (!portCopyDict.ContainsKey(edge.input) || !portCopyDict.ContainsKey(edge.output)) continue;
                var newEdge = PortHelper.ConnectPorts(portCopyDict[edge.output], portCopyDict[edge.input]);
                sourceView.AddElement(newEdge);
                copyElements.Add(newEdge);
            }
        }
        private void CopyGroupBlocks()
        {
            foreach (var selectBlock in sourceElements.OfType<DialogueGroup>())
            {
                var nodes = selectBlock.containedElements.Cast<IDialogueNode>();
                Rect newRect = selectBlock.GetPosition();
                newRect.position += new Vector2(50, 50);
                var block = sourceView.GroupBlockHandler.CreateGroup(newRect);
                block.title = selectBlock.title;
                block.AddElements(nodes.Where(x => nodeCopyDict.ContainsKey(x)).Select(x => nodeCopyDict[x]).Cast<Node>());
            }
        }
    }
}
