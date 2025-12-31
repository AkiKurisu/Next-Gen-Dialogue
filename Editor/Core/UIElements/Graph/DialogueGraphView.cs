using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.Reflection;
using Ceres.Editor.Graph;
using Ceres.Graph;
using USearchWindow = UnityEditor.Experimental.GraphView.SearchWindow;
using UObject = UnityEngine.Object;

namespace NextGenDialogue.Graph.Editor
{
    public partial class DialogueGraphView : CeresGraphView
    {
        private readonly DialogueGraph _graphInstance;
        
        public IDialogueGraphContainer DialogueGraphContainer { get; }

        private RootNodeView _root;

        internal Action<IDialogueNodeView> OnSelectNode;

        private const string InfoText = "Next-Gen Dialogue Graph Editor";

        private readonly CeresInfoContainer _infoContainer;
        
        /// <summary>
        /// Cached mouse position for paste operations
        /// </summary>
        private Vector2 _cachedMousePosition;

        private DialogueGraphEditorTracker _editorTracker;

        public DialogueGraphView(CeresGraphEditorWindow editorWindow) : base(editorWindow)
        {
            DialogueGraphContainer = (IDialogueGraphContainer)editorWindow.Container;
            _graphInstance = DialogueGraphContainer.GetDialogueGraph();
            styleSheets.Add(GetOrLoadStyleSheet(NextGenDialogueSettings.GraphStylePath));
            AddBlackboard(new DialogueBlackboard(this));
            Add(_infoContainer = new CeresInfoContainer(InfoText));
            AddSearchWindow<DialogueNodeSearchWindow>();
            AddNodeGroupHandler(new DialogueNodeGroupHandler(this));
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            
            // Initialize editor tracker for visualizing dialogue execution
            _editorTracker = new DialogueGraphEditorTracker(this);
            DialogueGraphTracker.SetActiveTracker(_editorTracker);
        }

        public override void OpenSearch(Vector2 screenPosition)
        {
            SearchWindow.Initialize(this, NodeSearchContext.Default);
            USearchWindow.Open(new SearchWindowContext(screenPosition), SearchWindow);
        }

        public override void OpenSearch(Vector2 screenPosition, CeresPortView portView)
        {
            /* Ports are not supported in dialogue graph */
            OpenSearch(screenPosition);
        }

        protected override string OnCopySerializedGraph(IEnumerable<GraphElement> elements)
        {
            // Serialize selected nodes to DialogueGraph model
            var selectedNodes = elements.OfType<IDialogueNodeView>().ToList();
            var tempGraph = SerializeGraph(selectedNodes);
            return tempGraph.GetData().ToJson();
        }

        protected override void OnPasteSerializedGraph(string operationName, string serializedData)
        {
            DeserializeGraph(serializedData, _cachedMousePosition);
        }
        
        private void OnMouseMoveEvent(MouseMoveEvent evt)
        {
            _cachedMousePosition = evt.mousePosition;
        }

        private DialogueGraph SerializeGraph(List<IDialogueNodeView> selectedNodes)
        {
            if (selectedNodes.Count == 0)
            {
                return new DialogueGraph();
            }

            var root = new Root();
            var serializeVisitor = new SerializeVisitor(this, root);
            var graph = serializeVisitor.Serialize(selectedNodes);
            
            // Collect variables used by selected nodes
            var usedVariables = new HashSet<string>();
            foreach (var nodeView in selectedNodes)
            {
                CollectUsedVariables(nodeView, usedVariables);
            }
            
            // Add variables to graph
            graph.variables = SharedVariables
                .Where(variable => variable != null && usedVariables.Contains(variable.Name))
                .Select(variable => variable.Clone())
                .ToList();
            
            // Collect node groups containing selected nodes
            var nodeGroups = new List<NodeGroup>();
            var groupBlocks = graphElements.OfType<DialogueNodeGroup>().ToList();
            foreach (var block in groupBlocks)
            {
                var containedSelectedNodes = block.containedElements
                    .OfType<IDialogueNodeView>()
                    .Where(selectedNodes.Contains)
                    .ToList();
                
                if (containedSelectedNodes.Count > 0)
                {
                    var nodeGroup = new NodeGroup
                    {
                        title = block.title,
                        position = block.GetPosition().position
                    };
                    block.Commit(new List<NodeGroup> { nodeGroup });
                    nodeGroups.Add(nodeGroup);
                }
            }
            graph.nodeGroups = nodeGroups;
            return graph;
        }

        private static void CollectUsedVariables(IDialogueNodeView nodeView, HashSet<string> usedVariables)
        {
            // Collect variable names from field resolvers
            var nodeType = nodeView.NodeType;
            var fields = nodeType.GetFields(BindingFlags.Public | 
                                           BindingFlags.NonPublic | 
                                           BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                if (fieldType.IsSubclassOf(typeof(SharedVariable)))
                {
                    var resolver = nodeView.GetFieldResolver(field.Name);
                    if (resolver?.Value is SharedVariable sharedVar && !string.IsNullOrEmpty(sharedVar.Name))
                    {
                        usedVariables.Add(sharedVar.Name);
                    }
                }
            }
        }

        public override void AddNodeView(ICeresNodeView nodeView)
        {
            var dialogueNode = (IDialogueNodeView)nodeView;
            base.AddNodeView(dialogueNode);
            nodeView.NodeElement.RegisterCallback<MouseDownEvent>(_ => OnNodeClick(dialogueNode));
        }

        private void OnNodeClick(IDialogueNodeView nodeView)
        {
            _infoContainer.DisplayNodeInfo(nodeView.NodeType);
            OnSelectNode(nodeView);
        }

        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                return e switch
                {
                    CeresDropdownMenuAction _ => true,
                    DropdownMenuAction a => a.name is "Create Node" or "Delete",
                    _ => false,
                };
            });
            // Remove needless default actions
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
            ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Graph, evt);
        }

        public sealed override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return PortHelper.GetCompatiblePorts(this, startAnchor);
        }

        protected override void OnDragDropObjectPerform(UObject data, Vector2 mousePosition)
        {
            if (data is GameObject gameObject)
            {
                if (gameObject.TryGetComponent(out IDialogueGraphContainer tree))
                {
                    EditorWindow.ShowNotification(new GUIContent("GameObject Dropped Succeed"));
                    DeserializeGraph(tree.GetDialogueGraph(), mousePosition);
                    return;
                }
                EditorWindow.ShowNotification(new GUIContent("Invalid Drag GameObject!"));
                return;
            }
            
            if (data is TextAsset asset)
            {
                EditorWindow.ShowNotification(DeserializeGraph(asset.text, mousePosition)
                    ? new GUIContent("Text Asset Dropped Succeed")
                    : new GUIContent("Invalid Drag Text Asset!"));
                return;
            }
            
            if (data is not IDialogueGraphContainer container)
            {
                EditorWindow.ShowNotification(new GUIContent("Invalid Drag Data!"));
                return;
            }
            
            EditorWindow.ShowNotification(new GUIContent("Data Dropped Succeed"));
            DeserializeGraph(container.GetDialogueGraph(), mousePosition);
        }
        
        public override void AddSharedVariables(List<SharedVariable> variables, bool duplicateWhenConflict)
        {
            if (Blackboard == null) return;
            
            foreach (var variable in variables.Where(variable => variable != null))
            {
                bool canAdd = SharedVariables.All(sharedVariable => sharedVariable.Name != variable.Name);
                // Piece id should always be unique
                if (variable is not PieceID)
                {
                    canAdd |= duplicateWhenConflict;
                }
                if (!canAdd) continue;
                
                // In play mode, use original variable to observe value change
                Blackboard.AddVariable(Application.isPlaying ? variable : variable.Clone(), false);
            }
        }

        private void ResolvePieceConflicts(DialogueGraph dialogueGraph)
        {
            var nameMapping = new Dictionary<string, string>();

            foreach (var variable in dialogueGraph.variables.OfType<PieceID>().ToArray())
            {
                var oldName = variable.Name;
                var newName = Blackboard.GetValidVariableName(variable);
                if (oldName != newName)
                {
                    nameMapping[oldName] = newName;
                }
            }

            foreach (var node in dialogueGraph.nodes)
            {
                if (node == null) continue;
                
                var nodeType = node.GetType();
                var fields = nodeType.GetFields(BindingFlags.Public | 
                                               BindingFlags.NonPublic | 
                                               BindingFlags.Instance);
                
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(PieceID))
                    {
                        if (field.GetValue(node) is PieceID pieceID && !string.IsNullOrEmpty(pieceID.Name))
                        {
                            // If this PieceID's name was renamed, update it
                            if (nameMapping.TryGetValue(pieceID.Name, out var newName))
                            {
                                pieceID.Name = newName;
                            }
                        }
                    }
                }
            }
        }

        private void DeserializeGraph(DialogueGraph graph, Vector2 mousePosition)
        {
            var graphMousePosition = contentViewContainer.WorldToLocal(mousePosition);
            Vector2 offset = CalculatePasteOffset(graph, graphMousePosition);
            graph.ApplyOffsetToGraph(offset);
            ResolvePieceConflicts(graph);
            AddSharedVariables(graph.variables, false);
            _ = DeserializeGraph(graph, this, true);
            NodeGroupHandler.RestoreGroups(graph.nodeGroups);
        }

        private static Vector2 CalculatePasteOffset(DialogueGraph graph, Vector2 targetMousePosition)
        {
            var centroid = CalculateGraphCentroid(graph);
            return targetMousePosition - centroid;
        }

        private static Vector2 CalculateGraphCentroid(DialogueGraph graph)
        {
            Vector2 sum = Vector2.zero;
            int count = 0;

            // Sum up all node positions
            foreach (var nodeInstance in graph.nodes)
            {
                if (nodeInstance is Root) continue;
                sum += nodeInstance.GraphPosition.position;
                count++;
            }

            // Return average position (centroid)
            return count > 0 ? sum / count : Vector2.zero;
        }

        public void RestoreGraph()
        {
            // Add default dialogue
            if (_graphInstance.Root == null)
            {
                _graphInstance.nodes.Add(new Root());
            }
            if (_graphInstance.Root!.GetActiveDialogue() == null)
            {
                var dialogue = new Dialogue();
                var pos = _graphInstance.Root.NodeData.graphPosition;
                pos.x += 200;
                dialogue.NodeData.graphPosition = pos;
                _graphInstance.Root.AddChild(dialogue);
            }
            
            AddSharedVariables(_graphInstance.variables, true);
            _root = DeserializeGraph(_graphInstance, this, false);
            NodeGroupHandler.RestoreGroups(_graphInstance.nodeGroups);
        }

        /// <summary>
        /// Dispose the editor tracker
        /// </summary>
        public void DisposeTracker()
        {
            _editorTracker?.Dispose();
            _editorTracker = null;
        }

        public bool SerializeGraph()
        {
            if (Application.isPlaying)
            {
                return false;
            }

            SerializeGraph(DialogueGraphContainer);
            AssetDatabase.SaveAssets();
            return true;
        }

        private void SerializeGraph(IDialogueGraphContainer container)
        {
            Undo.RecordObject(container.Object, "Commit dialogue graph change");
            var serializeVisitor = new SerializeVisitor(this, (Root)_root.CompileNode());
            var graph = serializeVisitor.Serialize(_root);

            // Commit variables
            graph.variables = SharedVariables.Where(variable => variable != null).ToList();

            // Commit blocks
            graph.nodeGroups = new List<NodeGroup>();
            graphElements.OfType<DialogueNodeGroup>()
                .ToList()
                .ForEach(block => block.Commit(graph.nodeGroups));

            container.SetGraphData(graph.GetData());

            // Should set component dirty flag if it is in a prefab
            if (container is NextGenDialogueComponent component)
            {
                EditorUtility.SetDirty(component);
            }
            // Notify unity editor that container is dirty
            EditorUtility.SetDirty(container.Object);
        }

        private bool DeserializeGraph(string serializedData, Vector2 mouseWorldPosition)
        {
            try
            {
                var data = CeresGraphData.FromJson<DialogueGraphData>(serializedData);
                if (data == null)
                {
                    return false;
                }
                
                DeserializeGraph(new DialogueGraph(data), mouseWorldPosition);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        private interface IParentAdapter
        {
            void Connect(DialogueGraphView graphView, IDialogueNodeView nodeToConnect);
        }

        private class PortAdapter : IParentAdapter
        {
            private readonly Port _port;

            public PortAdapter(Port port)
            {
                _port = port;
            }

            public void Connect(DialogueGraphView graphView, IDialogueNodeView nodeToConnect)
            {
                var edge = PortHelper.ConnectPorts(_port, nodeToConnect.Parent);
                graphView.Add(edge);
            }
        }

        private class ContainerAdapter : IParentAdapter
        {
            private readonly ContainerNodeView _container;

            public ContainerAdapter(ContainerNodeView container)
            {
                _container = container;
            }

            public void Connect(DialogueGraphView graphView, IDialogueNodeView nodeToConnect)
            {
                if (nodeToConnect is ModuleNodeView moduleNode)
                    _container.AddElement(moduleNode);
                else if (_container is IContainChildNode childContainer)
                    childContainer.AddChildElement(nodeToConnect);
            }
        }

        private readonly struct EdgePair
        {
            public readonly DialogueNode NodeInstance;

            public readonly IParentAdapter Adapter;

            public EdgePair(DialogueNode nodeInstance, IParentAdapter adapter)
            {
                NodeInstance = nodeInstance;
                Adapter = adapter;
            }
        }

        private static RootNodeView DeserializeGraph(DialogueGraph graph, DialogueGraphView graphView, bool copyPaste)
        {
            var stack = new Stack<EdgePair>();
            var alreadyCreateNodes = new Dictionary<DialogueNode, IDialogueNodeView>();
            RootNodeView root = null;
            stack.Push(new EdgePair(graph.Root, null));
            while (stack.Count > 0)
            {
                // create node
                var edgePair = stack.Pop();
                var instance = edgePair.NodeInstance;
                if (instance == null)
                {
                    continue;
                }
                
                // Prevent duplicating instance
                if (alreadyCreateNodes.TryGetValue(instance, out var nodeView))
                {
                    // connect parent
                    edgePair.Adapter?.Connect(graphView, nodeView);
                    continue;
                }

                var nodeType = instance.GetType();
                if (!copyPaste || nodeType != typeof(Root))
                {
                    nodeView = (IDialogueNodeView)NodeViewFactory.Get().CreateInstance(nodeType, graphView);
                    nodeView.SetNodeInstance(instance);
                    graphView.AddNodeView(nodeView);
                    nodeView.NodeElement.SetPosition(instance.GraphPosition);
                    alreadyCreateNodes.Add(instance, nodeView);

                    // connect parent
                    edgePair.Adapter?.Connect(graphView, nodeView);
                }

                // seek child
                switch (instance)
                {
                    case ContainerNode nb:
                        {
                            var containerNode = (ContainerNodeView)nodeView;
                            for (var i = nb.Children.Count - 1; i >= 0; i--)
                            {
                                stack.Push(new EdgePair(nb.Children[i], new ContainerAdapter(containerNode)));
                            }
                            break;
                        }
                    case Root nb:
                        {
                            if (copyPaste)
                            {
                                foreach (var childNode in nb.Children)
                                {
                                    stack.Push(new EdgePair(childNode, null));
                                }
                                break;
                            }
                            
                            root = (RootNodeView)nodeView;
                            var activeDialogue = nb.GetActiveDialogue();
                            if (activeDialogue != null)
                            {
                                stack.Push(new EdgePair(activeDialogue, new PortAdapter(root.Child)));
                                foreach (var childNode in nb.Children.Except(new []{ activeDialogue }))
                                {
                                    stack.Push(new EdgePair(childNode, null));
                                }
                                break;
                            }
                            
                            // No active dialogue
                            foreach (var childNode in nb.Children)
                            {
                                stack.Push(new EdgePair(childNode, null));
                            }
                            break;
                        }
                }
            }
            return root;
        }
    }
}