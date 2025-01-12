using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class CopyPasteGraphConvertor
    {
        private readonly DialogueGraphView sourceView;
        
        private readonly List<ISelectable> copyElements;
        
        private readonly Dictionary<Port, Port> portCopyDict;
        
        private readonly Dictionary<IDialogueNode, IDialogueNode> nodeCopyDict;
        
        private readonly List<GraphElement> sourceElements;
        
        private readonly HashSet<Edge> sourceEdges;
        public CopyPasteGraphConvertor(DialogueGraphView sourceView, List<GraphElement> sourceElements, Vector2 positionOffSet)
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
                        portCopyDict.Add(behaviorModuleNode.Child, ((BehaviorModuleNode)copyMap[x.GetHashCode()]).Child);
                    else if (x is ChildBridge childBridge)
                        portCopyDict.Add(childBridge.Child, ((ChildBridge)copyMap[x.GetHashCode()]).Child);
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
                var behaviorModuleNode = (BehaviorModuleNode)sourceNode;
                portCopyDict.Add(behaviorModuleNode.Child, ((BehaviorModuleNode)pasteNode).Child);
            }
            else if (behaviorType.IsSubclassOf(typeof(Action)))
            {
                var actionNode = (ActionNode)sourceNode;
                portCopyDict.Add(actionNode.Parent, ((ActionNode)pasteNode).Parent);
            }
            else if (behaviorType.IsSubclassOf(typeof(Composite)))
            {
                var compositeNode = (CompositeNode)sourceNode;
                var copy = (CompositeNode)pasteNode;
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
                var conditionalNode = (ConditionalNode)sourceNode;
                portCopyDict.Add(conditionalNode.Child, ((ConditionalNode)pasteNode).Child);
                portCopyDict.Add(conditionalNode.Parent, ((ConditionalNode)pasteNode).Parent);

            }
            else if (behaviorType.IsSubclassOf(typeof(Decorator)))
            {
                var decoratorNode = (DecoratorNode)pasteNode;
                portCopyDict.Add(decoratorNode.Child, ((DecoratorNode)pasteNode).Child);
                portCopyDict.Add(decoratorNode.Parent, ((DecoratorNode)pasteNode).Parent);
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
            foreach (var selectBlock in sourceElements.OfType<DialogueNodeGroup>())
            {
                var nodes = selectBlock.containedElements.Cast<IDialogueNode>();
                Rect newRect = selectBlock.GetPosition();
                newRect.position += new Vector2(50, 50);
                var block = sourceView.NodeGroupHandler.CreateGroup(newRect);
                block.title = selectBlock.title;
                block.AddElements(nodes.Where(x => nodeCopyDict.ContainsKey(x)).Select(x => nodeCopyDict[x]).Cast<Node>());
            }
        }
    }
}
