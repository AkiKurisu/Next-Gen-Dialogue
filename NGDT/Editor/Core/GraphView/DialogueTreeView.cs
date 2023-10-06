using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using System.Threading.Tasks;
namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeView : GraphView, IDialogueTreeView
    {
        public GraphView View => this;
        public IBlackBoard BlackBoard { get; internal set; }
        private readonly IDialogueTree behaviorTree;
        public IDialogueTree BehaviorTree => behaviorTree;
        private RootNode root;
        private readonly List<SharedVariable> exposedProperties = new();
        public List<SharedVariable> ExposedProperties => exposedProperties;
        private readonly NodeSearchWindowProvider provider;
        public event Action<SharedVariable> OnPropertyNameChange;
        public bool CanSaveToSO => behaviorTree is NextGenDialogueTree;
        private readonly NodeResolverFactory nodeResolver = NodeResolverFactory.Instance;
        public Action<IDialogueNode> OnSelectAction { get; internal set; }
        private readonly EditorWindow _window;
        private readonly NodeConverter converter = new();
        private readonly DragDropManipulator dragDropManipulator;
        private readonly AIDialogueBaker baker = new();
        public DialogueTreeView(IDialogueTree bt, EditorWindow editor)
        {
            _window = editor;
            behaviorTree = bt;
            style.flexGrow = 1;
            style.flexShrink = 1;
            styleSheets.Add(NextGenDialogueSetting.GetGraphStyle());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            Insert(0, new GridBackground());
            var contentDragger = new ContentDragger();
            contentDragger.activators.Add(new ManipulatorActivationFilter()
            {
                button = MouseButton.MiddleMouse,
            });
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());
            this.AddManipulator(contentDragger);
            dragDropManipulator = new DragDropManipulator(this);
            dragDropManipulator.OnDragOverEvent += CopyFromObject;
            this.AddManipulator(dragDropManipulator);
            provider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();
            provider.Initialize(this, editor, NextGenDialogueSetting.GetMask());
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
            };
            serializeGraphElements += CopyOperation;
            canPasteSerializedData += (data) => true;
            unserializeAndPaste += OnPaste;
        }
        private string CopyOperation(IEnumerable<GraphElement> elements)
        {
            ClearSelection();
            foreach (GraphElement n in elements)
            {
                AddToSelection(n);
            }
            return "Copy Nodes";
        }
        private void OnPaste(string a, string b)
        {
            List<ISelectable> copyElements = new CopyPasteGraph(this, selection).GetCopyElements();
            ClearSelection();
            //Select them again
            copyElements.ForEach(node =>
            {
                node.Select(this, true);
            });
        }
        public IDialogueNode DuplicateNode(IDialogueNode node)
        {
            var newNode = nodeResolver.Create(node.GetBehavior(), this);
            if (newNode is PieceContainer pieceContainer)
            {
                pieceContainer.GenerateNewPieceID();
            }
            var nodeElement = newNode as Node;
            Rect newRect = node.GetWorldPosition();
            newRect.position += new Vector2(50, 50);
            nodeElement.SetPosition(newRect);
            newNode.OnSelectAction = OnSelectAction;
            AddElement(nodeElement);
            newNode.CopyFrom(node);
            return newNode;
        }
        public GroupBlock CreateBlock(Rect rect, GroupBlockData blockData = null)
        {
            blockData ??= new GroupBlockData();
            var group = new GroupBlock
            {
                autoUpdateGeometry = true,
                title = blockData.Title
            };
            AddElement(group);
            group.SetPosition(rect);
            return group;
        }
        internal void NotifyEditSharedVariable(SharedVariable variable)
        {
            OnPropertyNameChange?.Invoke(variable);
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                return e switch
                {
                    NGDTDropdownMenuAction _ => true,
                    DropdownMenuAction a => a.name == "Create Node" || a.name == "Delete",
                    _ => false,
                };
            });
            //Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
        }
        public sealed override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return PortHelper.GetCompatiblePorts(this, startAnchor);
        }
        private void CopyFromObject(UnityEngine.Object data, Vector3 mousePosition)
        {
            if (data is GameObject)
            {
                if ((data as GameObject).TryGetComponent(out IDialogueTree tree))
                {
                    _window.ShowNotification(new GUIContent("GameObject Dropped Succeed !"));
                    CopyFromOtherTree(tree, mousePosition);
                    return;
                }
                _window.ShowNotification(new GUIContent("Invalid Drag GameObject !"));
                return;
            }
            if (data is TextAsset asset)
            {
                if (CopyFromJson(asset.text, mousePosition))
                    _window.ShowNotification(new GUIContent("Text Asset Dropped Succeed !"));
                else
                    _window.ShowNotification(new GUIContent("Invalid Drag Text Asset !"));
                return;
            }
            if (data is not IDialogueTree)
            {
                _window.ShowNotification(new GUIContent("Invalid Drag Data !"));
                return;
            }
            _window.ShowNotification(new GUIContent("Data Dropped Succeed !"));
            CopyFromOtherTree(data as IDialogueTree, mousePosition);
        }
        internal void CopyFromOtherTree(IDialogueTree otherTree, Vector2 mousePosition)
        {
            var localMousePosition = contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
            IEnumerable<IDialogueNode> nodes;
            RootNode rootNode;
            foreach (var variable in otherTree.SharedVariables)
            {
                BlackBoard.AddExposedProperty(variable.Clone() as SharedVariable, true);
            }
            (rootNode, nodes) = converter.ConvertToNode(otherTree, this, localMousePosition);
            foreach (var node in nodes) node.OnSelectAction = OnSelectAction;
            var edge = rootNode.Child.connections.First();
            RemoveElement(edge);
            RemoveElement(rootNode);
            RestoreBlocks(otherTree, nodes);
        }
        internal void Restore()
        {
            IDialogueTree tree = behaviorTree.ExternalBehaviorTree ?? behaviorTree;
            OnRestore(tree);
        }
        private void OnRestore(IDialogueTree tree)
        {
            //Add default dialogue
            if (tree.Root.Child == null)
            {
                tree.Root.Child = new Dialogue();
                var pos = tree.Root.graphPosition;
                pos.x += 200;
                tree.Root.Child.graphPosition = pos;
            }
            RestoreSharedVariables(tree);
            IEnumerable<IDialogueNode> nodes;
            (root, nodes) = converter.ConvertToNode(tree, this, Vector2.zero);
            foreach (var node in nodes) node.OnSelectAction = OnSelectAction;
            RestoreBlocks(tree, nodes);
        }
        private void RestoreSharedVariables(IDialogueTree tree)
        {
            foreach (var variable in tree.SharedVariables)
            {
                BlackBoard.AddExposedProperty(variable.Clone() as SharedVariable, false);
            }
        }
        private void RestoreBlocks(IDialogueTree tree, IEnumerable<IDialogueNode> nodes)
        {
            foreach (var nodeBlockData in tree.BlockData)
            {
                CreateBlock(new Rect(nodeBlockData.Position, new Vector2(100, 100)), nodeBlockData)
                .AddElements(nodes.Where(x => nodeBlockData.ChildNodes.Contains(x.GUID)).Cast<Node>());
            }
        }
        void IDialogueTreeView.SelectGroup(IDialogueNode node)
        {
            var block = CreateBlock(new Rect((node as Node).transform.position, new Vector2(100, 100)));
            foreach (var select in selection)
            {
                if (select is RootNode || select is not Node graphNode) continue;
                block.AddElement(graphNode);
            }
        }
        void IDialogueTreeView.UnSelectGroup()
        {
            foreach (var select in selection)
            {
                if (select is not Node graphNode) continue;
                var block = graphElements.OfType<GroupBlock>().FirstOrDefault(x => x.ContainsElement(graphNode));
                block?.RemoveElement(graphNode);
            }
        }

        internal bool Save(bool autoSave = false)
        {
            if (Application.isPlaying) return false;
            if (Validate())
            {
                Commit(behaviorTree);
                if (autoSave) Debug.Log($"<color=#3aff48>NGDT</color>[{behaviorTree.Object.name}] auto save succeed ! {DateTime.Now}");
                AssetDatabase.SaveAssets();
                return true;
            }
            if (autoSave) Debug.Log($"<color=#ff2f2f>NGDT</color>[{behaviorTree.Object.name}] auto save failed ! {DateTime.Now}");
            return false;
        }

        private bool Validate()
        {
            //validate nodes by DFS.
            var stack = new Stack<IDialogueNode>();
            stack.Push(root);
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
        internal void Commit(IDialogueTree behaviorTree)
        {
            var stack = new Stack<IDialogueNode>();
            stack.Push(root);
            // save new components
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                node.Commit(stack);
            }
            root.PostCommit(behaviorTree);
            behaviorTree.SharedVariables.Clear();
            foreach (var sharedVariable in ExposedProperties)
            {
                behaviorTree.SharedVariables.Add(sharedVariable);
            }
            List<GroupBlock> NodeBlocks = graphElements.OfType<GroupBlock>().ToList();
            behaviorTree.BlockData.Clear();
            foreach (var block in NodeBlocks)
            {
                block.Commit(behaviorTree.BlockData);
            }
            // notify to unity editor that the tree is changed.
            EditorUtility.SetDirty(behaviorTree.Object);
        }
        internal string SerializeTreeToJson()
        {
            return SerializationUtility.SerializeTree(behaviorTree, false, true);
        }
        internal bool CopyFromJson(string serializedData, Vector3 mousePosition)
        {
            var temp = ScriptableObject.CreateInstance<NextGenDialogueTreeSO>();
            try
            {
                temp.Deserialize(serializedData);
                CopyFromOtherTree(temp, mousePosition);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async void BakeDialogue()
        {
            var containers = selection.OfType<ContainerNode>().ToList();
            if (containers.Count == 0) return;
            var bakeContainer = containers.Last();
            containers.Remove(bakeContainer);
            if (containers.Any(x => x.TryGetModuleNode<AIBakeModule>(out ModuleNode _)))
            {
                _window.ShowNotification(new GUIContent($"AIBakeModule should only be added to the last select node !"));
                return;
            }
            float startVal = (float)EditorApplication.timeSinceStartup;
            const float maxValue = 60.0f;
            var task = baker.Bake(containers, bakeContainer);
            while (!task.IsCompleted)
            {
                float slider = (float)(EditorApplication.timeSinceStartup - startVal) / maxValue;
                EditorUtility.DisplayProgressBar("Wait to bake dialogue", "Waiting for the few seconds", slider);
                if (slider > 1)
                {
                    _window.ShowNotification(new GUIContent($"Dialogue baking is out of time, please check your internet !"));
                    break;
                }
                await Task.Yield();
            }
            EditorUtility.ClearProgressBar();
        }
    }
}