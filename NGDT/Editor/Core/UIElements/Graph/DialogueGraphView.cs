using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Ceres.Graph;
using USearchWindow = UnityEditor.Experimental.GraphView.SearchWindow;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT.Editor
{
    public class DialogueGraphView : CeresGraphView
    {
        private readonly DialogueGraph _graphInstance;
        public IDialogueGraphContainer DialogueGraphContainer { get; }

        private RootNode _root;
        
        // ReSharper disable once InconsistentNaming
        internal Action<IDialogueNodeView> OnSelectNode;
        
        private const string InfoText = "Next-Gen Dialogue Graph Editor";

        private readonly CeresInfoContainer _infoContainer;
        
        public DialogueGraphView(CeresGraphEditorWindow editorWindow) : base(editorWindow)
        {
            DialogueGraphContainer = (IDialogueGraphContainer)editorWindow.Container;
            _graphInstance = DialogueGraphContainer.GetDialogueGraph();
            styleSheets.Add(GetOrLoadStyleSheet(NextGenDialogueSettings.GraphStylePath));
            AddBlackboard(new DialogueBlackboard(this));
            Add(_infoContainer = new CeresInfoContainer(InfoText));
            AddSearchWindow<DialogueNodeSearchWindow>();
            AddNodeGroupHandler(new DialogueNodeGroupHandler(this));
        }

        public override void OpenSearch(Vector2 screenPosition)
        {
            /* Override context from settings */
            SearchWindow.Initialize(this, NextGenDialogueSettings.GetNodeSearchContext());
            USearchWindow.Open(new SearchWindowContext(screenPosition), SearchWindow);
        }

        public override void OpenSearch(Vector2 screenPosition, CeresPortView portView)
        {
            /* Ports are not supported in dialogue graph */
            OpenSearch(screenPosition);
        }

        protected override string OnCopySerializedGraph(IEnumerable<GraphElement> elements)
        {
            CopyPasteGraph.Copy(EditorWindow.Identifier, elements.ToArray());
            return string.Empty;
        }

        protected override void OnPasteSerializedGraph(string operationName, string serializedData)
        {
            if (CopyPasteGraph.CanPaste)
            {
                Paste(new Vector2(50, 50));
            }
        }
        
        private void Paste(Vector2 positionOffSet)
        {
            ClearSelection();
            // Add paste elements to selection
            foreach (var element in new CopyPasteGraph(this, CopyPasteGraph.Paste(), positionOffSet).GetCopyElements())
            {
                element.Select(this, true);
            }
        }
        
        public IDialogueNodeView DuplicateNode(IDialogueNodeView node)
        {
            var newNode = (IDialogueNodeView)NodeViewFactory.Get().CreateInstance(node.GetBehavior(), this);
            if (newNode is PieceContainer pieceContainer)
            {
                pieceContainer.GenerateNewPieceID();
            }
            var nodeElement = newNode as Node;
            var newRect = node.GetWorldPosition();
            newRect.position += new Vector2(50, 50);
            nodeElement!.SetPosition(newRect);
            AddNodeView(newNode);
            newNode.CopyFrom(node);
            return newNode;
        }

        public override void AddNodeView(ICeresNodeView nodeView)
        {
            var dialogueNode = (IDialogueNodeView)nodeView;
            base.AddNodeView(dialogueNode);
            nodeView.NodeElement.RegisterCallback<MouseDownEvent>(_ => OnNodeClick(dialogueNode));
        }
        
        private void OnNodeClick(IDialogueNodeView nodeView)
        {
            _infoContainer.DisplayNodeInfo(nodeView.GetBehavior());
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
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Paste", action =>
            {
                Paste(contentViewContainer.WorldToLocal(action.eventInfo.mousePosition) - CopyPasteGraph.CenterPosition);
            }, x => CopyPasteGraph.CanPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
            ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Graph, evt, null);
        }
        
        public sealed override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return PortHelper.GetCompatiblePorts(this, startAnchor);
        }
        
        protected override void OnDragDropObjectPerform(UObject data, Vector3 mousePosition)
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
                if (DeserializeGraph(asset.text, mousePosition))
                    EditorWindow.ShowNotification(new GUIContent("Text Asset Dropped Succeed"));
                else
                    EditorWindow.ShowNotification(new GUIContent("Invalid Drag Text Asset!"));
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
        
        public void DeserializeGraph(DialogueGraph graph, Vector2 mousePosition)
        {
            var localMousePosition = contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
            foreach (var variable in graph.variables)
            {
                Blackboard.AddVariable(variable.Clone(), false);
            }
            var rootNode = DeserializeGraph(graph, this, localMousePosition);
            // Remove root from external graph
            var edge = rootNode.Child.connections.First();
            RemoveElement(edge);
            RemoveElement(rootNode);
            // Restore node groups
            NodeGroupHandler.RestoreGroups(graph.nodeGroups);
        }
        
        public void Restore()
        {
            // Add default dialogue
            if (_graphInstance.Root == null)
            {
                _graphInstance.nodes.Add(new Root());
            }
            if (_graphInstance.Root!.Child == null)
            {
                _graphInstance.Root.Child = new Dialogue();
                var pos = _graphInstance.Root.NodeData.graphPosition;
                pos.x += 200;
                _graphInstance.Root.Child.NodeData.graphPosition = pos;
            }
            AddSharedVariables(_graphInstance.variables,true);
            _root = DeserializeGraph(_graphInstance, this, Vector2.zero);
            // Restore node groups
            NodeGroupHandler.RestoreGroups(_graphInstance.nodeGroups);
        }

        public bool Save()
        {
            if (Application.isPlaying) return false;
            if (!Validate()) return false;
            
            Commit(DialogueGraphContainer);
            AssetDatabase.SaveAssets();
            return true;
        }

        public bool Validate()
        {
            // validate nodes by DFS.
            var stack = new Stack<IDialogueNodeView>();
            stack.Push(_root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                if (!node.Validate(stack))
                {
                    return false;
                }
            }
            return true;
        }
        
        public void Commit(IDialogueGraphContainer container)
        {
            Undo.RecordObject(container.Object, "Commit dialogue graph change");
            var stack = new Stack<IDialogueNodeView>();
            var graph = new DialogueGraph();
            
            // Commit node instances
            stack.Push(_root);
            while (stack.Count > 0)
            {
                stack.Pop().Commit(stack);
            }
            _root.PostCommit(graph);

            // Commit variables
            graph.variables = SharedVariables.Where(x=>x != null).ToList();
            
            // Commit blocks
            graph.nodeGroups = new List<NodeGroup>();
            var groupBlocks = graphElements.OfType<DialogueNodeGroup>().ToList();
            foreach (var block in groupBlocks)
            {
                block.Commit(graph.nodeGroups);
            }
            
            container.SetGraphData(graph.GetData());
            
            // Should set component dirty flag if it is in a prefab
            if (container is NextGenDialogueComponent component)
            {
                EditorUtility.SetDirty(component);
            }
            // Notify unity editor that container is dirty
            EditorUtility.SetDirty(container.Object);
        }

        public string SerializeGraph()
        {
            return _graphInstance.GetData().ToJson(true);
        }
        
        public bool DeserializeGraph(string serializedData, Vector3 mousePosition)
        {
            var temp = ScriptableObject.CreateInstance<NextGenDialogueGraphAsset>();
            try
            {
                temp.Deserialize(serializedData);
                DeserializeGraph(temp.GetDialogueGraph(), mousePosition);
                return true;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
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
            private readonly ContainerNode _container;
            
            public ContainerAdapter(ContainerNode container)
            {
                _container = container;
            }
            
            public void Connect(DialogueGraphView graphView, IDialogueNodeView nodeToConnect)
            {
                if (nodeToConnect is ModuleNode moduleNode)
                    _container.AddElement(moduleNode);
                else if (_container is IContainChild childContainer)
                    childContainer.AddChildElement(nodeToConnect, graphView);
            }
        }
        
        private readonly struct EdgePair
        {
            public readonly NGDT.DialogueNode NodeBehavior;
            
            public readonly IParentAdapter Adapter;

            public EdgePair(NGDT.DialogueNode nodeBehavior, IParentAdapter adapter)
            {
                NodeBehavior = nodeBehavior;
                Adapter = adapter;
            }
        }
        
        private static RootNode DeserializeGraph(DialogueGraph graph, DialogueGraphView graphView, Vector2 initPos)
        {
            var stack = new Stack<EdgePair>();
            var alreadyCreateNodes = new Dictionary<NGDT.DialogueNode, IDialogueNodeView>();
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
                
                nodeView = (IDialogueNodeView)NodeViewFactory.Get().CreateInstance(edgePair.NodeBehavior.GetType(), graphView);
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
                    case Root nb:
                        {
                            root = (RootNode)nodeView;
                            if (nb.Child != null)
                            {
                                stack.Push(new EdgePair(nb.Child, new PortAdapter(root.Child)));
                            }
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