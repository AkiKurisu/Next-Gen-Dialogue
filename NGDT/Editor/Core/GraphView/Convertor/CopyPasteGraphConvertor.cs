using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class CopyPasteGraphConvertor
    {
        private readonly IDialogueTreeView sourceView;
        private readonly List<ISelectable> copyElements;
        private readonly Dictionary<Port, Port> portCopyDict;
        private readonly Dictionary<IDialogueNode, IDialogueNode> nodeCopyDict;
        private readonly List<ISelectable> selection;
        public CopyPasteGraphConvertor(IDialogueTreeView sourceView, List<ISelectable> selection)
        {
            this.sourceView = sourceView;
            this.selection = selection;
            copyElements = new List<ISelectable>();
            portCopyDict = new Dictionary<Port, Port>();
            nodeCopyDict = new Dictionary<IDialogueNode, IDialogueNode>();
            DistinctNodes();
            CopyNodes();
            CopyEdges();
            CopyGroupBlocks();
        }
        public List<ISelectable> GetCopyElements() => copyElements;
        private void DistinctNodes()
        {
            var containerNodes = selection.OfType<ContainerNode>().ToArray();
            foreach (var containerNode in containerNodes)
            {
                containerNode.contentContainer.Query<ModuleNode>().ForEach(x => selection.Remove(x));
            }
        }
        private void CopyNodes()
        {
            foreach (var select in selection)
            {
                if (select is not IDialogueNode selectNode) continue;
                var node = sourceView.DuplicateNode(selectNode);
                copyElements.Add(node as Node);
                nodeCopyDict.Add(selectNode, node);
                CopyPort(selectNode, node);
            }
        }
        private void CopyPort(IDialogueNode selectNode, IDialogueNode node)
        {
            var behaviorType = selectNode.GetBehavior();
            if (behaviorType.IsSubclassOf(typeof(Container)))
            {
                var containerNode = selectNode as ContainerNode;
                var copyMap = (node as ContainerNode).GetModuleCopyMap();
                containerNode.contentContainer.Query<BehaviorModuleNode>()
                .ToList()
                .ForEach(x =>
                {
                    portCopyDict.Add(x.Child, (copyMap[x.GetHashCode()] as BehaviorModuleNode).Child);
                });
            }
            else if (behaviorType.IsSubclassOf(typeof(BehaviorModule)))
            {
                var behaviorModuleNode = selectNode as BehaviorModuleNode;
                portCopyDict.Add(behaviorModuleNode.Child, (node as BehaviorModuleNode).Child);
            }
            else if (behaviorType.IsSubclassOf(typeof(Action)))
            {
                var actionNode = selectNode as ActionNode;
                portCopyDict.Add(actionNode.Parent, (node as ActionNode).Parent);
            }
            else if (behaviorType.IsSubclassOf(typeof(Composite)))
            {
                var compositeNode = selectNode as CompositeNode;
                var copy = node as CompositeNode;
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
                var conditionalNode = selectNode as ConditionalNode;
                portCopyDict.Add(conditionalNode.Child, (node as ConditionalNode).Child);
                portCopyDict.Add(conditionalNode.Parent, (node as ConditionalNode).Parent);

            }
            else if (behaviorType.IsSubclassOf(typeof(Decorator)))
            {
                var decoratorNode = node as DecoratorNode;
                portCopyDict.Add(decoratorNode.Child, (node as DecoratorNode).Child);
                portCopyDict.Add(decoratorNode.Parent, (node as DecoratorNode).Parent);
            }
        }
        private void CopyEdges()
        {
            foreach (var select in selection)
            {
                if (select is not Edge) continue;
                var edge = select as Edge;
                if (!portCopyDict.ContainsKey(edge.input) || !portCopyDict.ContainsKey(edge.output)) continue;
                var newEdge = PortHelper.ConnectPorts(portCopyDict[edge.output], portCopyDict[edge.input]);
                sourceView.View.AddElement(newEdge);
                copyElements.Add(newEdge);
            }
        }
        private void CopyGroupBlocks()
        {
            foreach (var select in selection)
            {
                if (select is not GroupBlock) continue;
                var selectBlock = select as GroupBlock;
                var nodes = selectBlock.containedElements.Cast<IDialogueNode>();
                Rect newRect = selectBlock.GetPosition();
                newRect.position += new Vector2(50, 50);
                var block = sourceView.GroupBlockController.CreateBlock(newRect);
                block.title = selectBlock.title;
                block.AddElements(nodes.Where(x => nodeCopyDict.ContainsKey(x)).Select(x => nodeCopyDict[x]).Cast<Node>());
            }
        }
    }
}
