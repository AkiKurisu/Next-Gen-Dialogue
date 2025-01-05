using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Ceres;
using Ceres.Editor;
using Ceres.Editor.Graph;
using Ceres.Graph;
using USearchWindow = UnityEditor.Experimental.GraphView.SearchWindow;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT.Editor
{
    public class DialogueGraphView : CeresGraphView
    {
        public DialogueGraph Instance { get; }
        
        public IDialogueGraphContainer DialogueGraphContainer { get; }

        private RootNode _root;
        
        // ReSharper disable once InconsistentNaming
        internal Action<IDialogueNode> OnSelectNode;
        
        private readonly NodeConvertor _converter = new();
        
        private const string InfoText = "Welcome to Next-Gen Dialogue Editor!";

        private readonly CeresInfoContainer _infoContainer;
        
        public DialogueGraphView(CeresGraphEditorWindow editorWindow) : base(editorWindow)
        {
            DialogueGraphContainer = (IDialogueGraphContainer)editorWindow.Container;
            Instance = (DialogueGraph)DialogueGraphContainer.GetGraph();
            styleSheets.Add(NextGenDialogueSettings.GetGraphStyle());
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
            /* Port not support in dialogue graph view */
            OpenSearch(screenPosition);
        }

        protected override string OnSerialize(IEnumerable<GraphElement> elements)
        {
            CopyPaste.Copy(EditorWindow.GetWindowId(), elements.ToArray());
            return string.Empty;
        }

        protected override void OnPaste(string a, string b)
        {
            if (CopyPaste.CanPaste)
                Paste(new Vector2(50, 50));
        }
        
        private void Paste(Vector2 positionOffSet)
        {
            ClearSelection();
            // Add paste elements to selection
            foreach (var element in new CopyPasteGraphConvertor(this, CopyPaste.Paste(), positionOffSet).GetCopyElements())
            {
                element.Select(this, true);
            }
        }
        
        public IDialogueNode DuplicateNode(IDialogueNode node)
        {
            var newNode = DialogueNodeFactory.Get().Create(node.GetBehavior(), this);
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
            var dialogueNode = (IDialogueNode)nodeView;
            base.AddNodeView(dialogueNode);
            nodeView.NodeElement.RegisterCallback<MouseDownEvent>(evt => OnNodeClick(dialogueNode));
        }
        
        private void OnNodeClick(IDialogueNode nodeView)
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
                Paste(contentViewContainer.WorldToLocal(action.eventInfo.mousePosition) - CopyPaste.CenterPosition);
            }, x => CopyPaste.CanPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
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
                    DeserializeGraph((DialogueGraph)tree.GetGraph(), mousePosition);
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
            DeserializeGraph((DialogueGraph)container.GetGraph(), mousePosition);
        }
        
        public void DeserializeGraph(DialogueGraph graph, Vector2 mousePosition)
        {
            var localMousePosition = contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
            foreach (var variable in graph.variables)
            {
                Blackboard.AddVariable(variable.Clone(), false);
            }
            var (rootNode, newNodes) = _converter.ConvertToNode(graph, this, localMousePosition);
            var edge = rootNode.Child.connections.First();
            RemoveElement(edge);
            RemoveElement(rootNode);
            RestoreBlocks(graph, newNodes);
        }
        
        public void Restore()
        {
            // Add default dialogue
            if (Instance.Root.Child == null)
            {
                Instance.Root.Child = new Dialogue();
                var pos = Instance.Root.NodeData.graphPosition;
                pos.x += 200;
                Instance.Root.Child.NodeData.graphPosition = pos;
            }
            AddSharedVariables(Instance.variables,true);
            List<IDialogueNode> newNodes;
            (_root, newNodes) = _converter.ConvertToNode(Instance, this, Vector2.zero);
            RestoreBlocks(Instance, newNodes);
        }
        
        private void RestoreBlocks(DialogueGraph graph, List<IDialogueNode> inNodes)
        {
            foreach (var nodeGroup in graph.nodeGroups)
            {
                NodeGroupHandler.CreateGroup(new Rect(nodeGroup.position, new Vector2(100, 100)), nodeGroup)
                .AddElements(inNodes.Where(x => nodeGroup.childNodes.Contains(x.Guid)).Cast<Node>());
            }
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
            var stack = new Stack<IDialogueNode>();
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
            var stack = new Stack<IDialogueNode>();
            var graph = new DialogueGraph();
            
            // Commit node instances
            stack.Push(_root);
            while (stack.Count > 0)
            {
                stack.Pop().Commit(stack);
            }
            _root.PostCommit(graph);

            // Commit variables
            graph.variables = new List<SharedVariable>(SharedVariables);
            
            // Commit blocks
            graph.nodeGroups = new List<NodeGroup>();
            var groupBlocks = graphElements.OfType<DialogueNodeGroup>().ToList();
            foreach (var block in groupBlocks)
            {
                block.Commit(graph.nodeGroups);
            }
            
            container.SetGraphData(graph.GetData());
            
            // Should set component dirty flag if it is in a prefab
            if (container is NextGenDialogueGraphComponent nextGenDialogueTree)
            {
                EditorUtility.SetDirty(nextGenDialogueTree);
            }
            // Notify unity editor that the tree is changed.
            EditorUtility.SetDirty(container.Object);
        }

        public string SerializeGraph()
        {
            return Instance.Serialize(true);
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
    }
}