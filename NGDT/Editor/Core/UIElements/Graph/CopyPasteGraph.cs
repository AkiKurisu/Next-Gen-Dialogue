using System.Collections.Generic;
using System.Linq;
using Ceres.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kurisu.NGDT.Editor
{
    public class CopyPasteGraph
    {
        private static GraphStructure _graphStructure;
        
        /// <summary>
        /// Copy elements to global without serialization
        /// </summary>
        /// <param name="identifier">Identifier of Dialogue Graph</param>
        /// <param name="elements"></param>
        public static void Copy(CeresGraphIdentifier identifier, GraphElement[] elements)
        {
            _graphStructure = new GraphStructure
            {
                Identifier = identifier,
                Edges = elements.OfType<Edge>().ToArray(),
                Nodes = elements.OfType<Node>().ToArray()
            };
        }
        
        public static List<GraphElement> Paste()
        {
            var list = new List<GraphElement>();
            if (_graphStructure == null) return list;
            
            list.AddRange(_graphStructure.Edges);
            list.AddRange(_graphStructure.Nodes);
            return list;
        }
        
        public static bool CanPaste => _graphStructure?.IsValid() ?? false;

        public static Vector2 CenterPosition
        {
            get
            {
                if (_graphStructure == null) return Vector2.zero;
                var average = Vector2.zero;
                int count = 0;
                foreach (var node in _graphStructure.Nodes)
                {
                    if (node is ChildBridgeView or ParentBridgeView) continue;
                    count++;
                    if (node is ModuleNodeView moduleNode)
                    {
                        average += moduleNode.GetWorldPosition().position;
                    }
                    else
                    {
                        average += node.GetPosition().position;
                    }
                }
                return average / count;
            }
        }
        
        private class GraphStructure
        {
            public CeresGraphIdentifier Identifier;
            
            public Edge[] Edges;
            
            public Node[] Nodes;

            public bool IsValid()
            {
               return DialogueEditorWindow.EditorWindowRegistry.FindWindow(Identifier);
            }
        }
        
        private readonly DialogueGraphView _sourceView;
        
        private readonly List<ISelectable> _copyElements;
        
        private readonly Dictionary<Port, Port> _portCopyDict;
        
        private readonly Dictionary<IDialogueNodeView, IDialogueNodeView> _nodeCopyDict;
        
        private readonly List<GraphElement> _sourceElements;
        
        private readonly HashSet<Edge> _sourceEdges;
        
        public CopyPasteGraph(DialogueGraphView sourceView, List<GraphElement> sourceElements, Vector2 positionOffSet)
        {
            _sourceView = sourceView;
            _sourceElements = sourceElements;
            _copyElements = new List<ISelectable>();
            _portCopyDict = new Dictionary<Port, Port>();
            _nodeCopyDict = new Dictionary<IDialogueNodeView, IDialogueNodeView>();
            _sourceEdges = sourceElements.OfType<Edge>().ToHashSet();
            DistinctNodes();
            CopyNodes();
            CopyEdges();
            CopyGroupBlocks();
            foreach (var pair in _nodeCopyDict)
            {
                Rect newRect = pair.Key.GetWorldPosition();
                newRect.position += positionOffSet;
                pair.Value.NodeElement.SetPosition(newRect);
            }
        }
        
        public List<ISelectable> GetCopyElements() => _copyElements;
        
        private void DistinctNodes()
        {
            var containerNodes = _sourceElements.OfType<ContainerNodeView>().ToArray();
            foreach (var containerNode in containerNodes)
            {
                containerNode.contentContainer.Query<ModuleNodeView>().ForEach(x => _sourceElements.Remove(x));
            }
        }
        
        private void CopyNodes()
        {
            foreach (var selectNode in _sourceElements.OfType<IDialogueNodeView>())
            {
                var node = _sourceView.DuplicateNode(selectNode);
                _copyElements.Add(node as Node);
                _nodeCopyDict.Add(selectNode, node);
                CopyPort(selectNode, node);
            }
        }
        
        private void CopyPort(IDialogueNodeView sourceNode, IDialogueNodeView pasteNode)
        {
            var behaviorType = sourceNode.GetBehavior();
            if (behaviorType.IsSubclassOf(typeof(NGDT.ContainerNode)))
            {
                var sourceContainer = (ContainerNodeView)sourceNode;
                var pasteContainer = (ContainerNodeView)pasteNode;
                var copyMap = pasteContainer.GetCopyMap();
                sourceContainer.contentContainer.Query<Node>()
                .ToList()
                .ForEach(x =>
                {
                    if (x is BehaviorModuleNodeView behaviorModuleNode)
                        _portCopyDict.Add(behaviorModuleNode.Child, ((BehaviorModuleNodeView)copyMap[x.GetHashCode()]).Child);
                    else if (x is ChildBridgeView childBridge)
                        _portCopyDict.Add(childBridge.Child, ((ChildBridgeView)copyMap[x.GetHashCode()]).Child);
                });
                //For some reason edges connected to bridge's ports are not selected by graph view
                //Add edge manually
                _portCopyDict.Add(sourceContainer.Parent, pasteContainer.Parent);
                if (sourceContainer.Parent.connected)
                {
                    var edge = sourceContainer.Parent.connections.FirstOrDefault();
                    _sourceEdges.Add(edge);
                }
            }
            else if (behaviorType.IsSubclassOf(typeof(BehaviorModule)))
            {
                var behaviorModuleNode = (BehaviorModuleNodeView)sourceNode;
                _portCopyDict.Add(behaviorModuleNode.Child, ((BehaviorModuleNodeView)pasteNode).Child);
            }
            else if (behaviorType.IsSubclassOf(typeof(NGDT.ActionNode)))
            {
                var actionNode = (ActionNodeView)sourceNode;
                _portCopyDict.Add(actionNode.Parent, ((ActionNodeView)pasteNode).Parent);
            }
            else if (behaviorType.IsSubclassOf(typeof(NGDT.CompositeNode)))
            {
                var compositeNode = (CompositeNodeView)sourceNode;
                var copy = (CompositeNodeView)pasteNode;
                int count = compositeNode.ChildPorts.Count - copy.ChildPorts.Count;
                for (int i = 0; i < count; i++)
                {
                    copy.AddChild();
                }
                for (int i = 0; i < compositeNode.ChildPorts.Count; i++)
                {
                    _portCopyDict.Add(compositeNode.ChildPorts[i], copy.ChildPorts[i]);
                }
                _portCopyDict.Add(compositeNode.Parent, copy.Parent);
            }
        }
        
        private void CopyEdges()
        {
            foreach (var edge in _sourceEdges)
            {
                if (!_portCopyDict.ContainsKey(edge.input) || !_portCopyDict.TryGetValue(edge.output, out var port)) continue;
                var newEdge = PortHelper.ConnectPorts(port, _portCopyDict[edge.input]);
                _sourceView.AddElement(newEdge);
                _copyElements.Add(newEdge);
            }
        }
        
        private void CopyGroupBlocks()
        {
            foreach (var selectBlock in _sourceElements.OfType<DialogueNodeGroup>())
            {
                var nodes = selectBlock.containedElements.Cast<IDialogueNodeView>();
                Rect newRect = selectBlock.GetPosition();
                newRect.position += new Vector2(50, 50);
                var block = _sourceView.NodeGroupHandler.CreateGroup(newRect);
                block.title = selectBlock.title;
                block.AddElements(nodes.Where(x => _nodeCopyDict.ContainsKey(x)).Select(x => _nodeCopyDict[x]).Cast<Node>());
            }
        }
    }
}