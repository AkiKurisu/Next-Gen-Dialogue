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
        public List<SharedVariable> ExposedProperties { get; } = new();
        private readonly NodeSearchWindowProvider provider;
        private readonly NodeResolverFactory nodeResolver = NodeResolverFactory.Instance;
        public Action<IDialogueNode> OnSelectAction { get; internal set; }
        public EditorWindow EditorWindow { get; internal set; }
        private readonly NodeConvertor converter = new();
        private readonly DragDropManipulator dragDropManipulator;
        private readonly AIDialogueBaker baker = new();
        public IControlGroupBlock GroupBlockController { get; }
        public DialogueTreeView(IDialogueTree bt, EditorWindow editor)
        {
            EditorWindow = editor;
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
            provider.Initialize(this, NextGenDialogueSetting.GetMask());
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
            };
            GroupBlockController = new GroupBlockController(this);
            canPasteSerializedData += (data) => true;
            unserializeAndPaste += OnPaste;
        }
        private void OnPaste(string a, string b)
        {
            List<ISelectable> copyElements = new CopyPasteGraphConvertor(this, selection).GetCopyElements();
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
                    EditorWindow.ShowNotification(new GUIContent("GameObject Dropped Succeed !"));
                    CopyFromOtherTree(tree, mousePosition);
                    return;
                }
                EditorWindow.ShowNotification(new GUIContent("Invalid Drag GameObject !"));
                return;
            }
            if (data is TextAsset asset)
            {
                if (CopyFromJson(asset.text, mousePosition))
                    EditorWindow.ShowNotification(new GUIContent("Text Asset Dropped Succeed !"));
                else
                    EditorWindow.ShowNotification(new GUIContent("Invalid Drag Text Asset !"));
                return;
            }
            if (data is not IDialogueTree)
            {
                EditorWindow.ShowNotification(new GUIContent("Invalid Drag Data !"));
                return;
            }
            EditorWindow.ShowNotification(new GUIContent("Data Dropped Succeed !"));
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
                GroupBlockController.CreateBlock(new Rect(nodeBlockData.Position, new Vector2(100, 100)), nodeBlockData)
                .AddElements(nodes.Where(x => nodeBlockData.ChildNodes.Contains(x.GUID)).Cast<Node>());
            }
        }

        internal bool Save()
        {
            if (Application.isPlaying) return false;
            if (Validate())
            {
                Commit(behaviorTree);
                AssetDatabase.SaveAssets();
                return true;
            }
            return false;
        }

        internal bool Validate()
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
                EditorWindow.ShowNotification(new GUIContent($"AIBakeModule should only be added to the last select node !"));
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
                    EditorWindow.ShowNotification(new GUIContent($"Dialogue baking is out of time, please check your internet !"));
                    break;
                }
                await Task.Yield();
            }
            EditorUtility.ClearProgressBar();
        }
    }
}