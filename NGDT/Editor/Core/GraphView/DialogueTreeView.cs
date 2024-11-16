using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Ceres.Editor;
using Ceres.Graph;

namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeView : CeresGraphView
    {
        private readonly IDialogueTree dialogueTree;
        public IDialogueTree DialogueTree => dialogueTree;
        
        private RootNode root;

        private readonly NodeSearchWindowProvider provider;
        
        private readonly NodeResolverFactory nodeResolver = NodeResolverFactory.Instance;
        public Action<IDialogueNode> OnSelectAction { get; internal set; }
        
        private readonly NodeConvertor converter = new();
        
        private readonly DragDropManipulator dragDropManipulator;
        public IControlGroupBlock GroupBlockController { get; }
        public ContextualMenuController ContextualMenuController { get; }
        public DialogueTreeView(IDialogueTree bt, EditorWindow editor)
        {
            EditorWindow = editor;
            dialogueTree = bt;
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
            ContextualMenuController = new();
            GroupBlockController = new GroupBlockController(this);
            canPasteSerializedData += (data) => true;
            serializeGraphElements += OnSerialize;
            unserializeAndPaste += OnPaste;
        }

        private string OnSerialize(IEnumerable<GraphElement> elements)
        {
            CopyPaste.Copy(EditorWindow.GetInstanceID(), elements);
            return string.Empty;
        }

        private void OnPaste(string a, string b)
        {
            if (CopyPaste.CanPaste)
                Paste(new Vector2(50, 50));
        }
        private void Paste(Vector2 positionOffSet)
        {
            ClearSelection();
            //Add paste elements to selection
            foreach (var element in new CopyPasteGraphConvertor(this, CopyPaste.Paste(), positionOffSet).GetCopyElements())
            {
                element.Select(this, true);
            }
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
        public void AddNode(IDialogueNode node, Rect worldRect)
        {
            node.View.SetPosition(worldRect);
            AddElement(node.View);
            node.OnSelectAction = OnSelectAction;
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                return e switch
                {
                    CeresDropdownMenuAction _ => true,
                    DropdownMenuAction a => a.name == "Create Node" || a.name == "Delete",
                    _ => false,
                };
            });
            //Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Paste", (evt) =>
            {
                Paste(contentViewContainer.WorldToLocal(evt.eventInfo.mousePosition) - CopyPaste.CenterPosition);
            }, x => CopyPaste.CanPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
            ContextualMenuController.BuildContextualMenu(ContextualMenuType.Graph, evt, null);
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
        public void CopyFromOtherTree(IDialogueTree otherTree, Vector2 mousePosition)
        {
            var localMousePosition = contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
            IEnumerable<IDialogueNode> nodes;
            RootNode rootNode;
            foreach (var variable in otherTree.SharedVariables)
            {
                Blackboard.AddVariable(variable.Clone(), false);
            }
            (rootNode, nodes) = converter.ConvertToNode(otherTree, this, localMousePosition);
            foreach (var node in nodes) node.OnSelectAction = OnSelectAction;
            var edge = rootNode.Child.connections.First();
            RemoveElement(edge);
            RemoveElement(rootNode);
            RestoreBlocks(otherTree, nodes);
        }
        public void Restore()
        {
            if (DialogueTreeEditorUtility.TryGetExternalTree(DialogueTree, out IDialogueTree tree))
            {
                OnRestore(tree);
            }
            else
            {
                OnRestore(DialogueTree);
            }
        }
        private void OnRestore(IDialogueTree tree)
        {
            //Add default dialogue
            if (tree.Root.Child == null)
            {
                tree.Root.Child = new Dialogue();
                var pos = tree.Root.nodeData.graphPosition;
                pos.x += 200;
                tree.Root.Child.nodeData.graphPosition = pos;
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
                //In play mode, use original variable to observe value change
                if (Application.isPlaying)
                {
                    Blackboard.AddVariable(variable, false);
                }
                else
                {
                    Blackboard.AddVariable(variable.Clone(), false);
                }
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

        public bool Save()
        {
            if (Application.isPlaying) return false;
            if (Validate())
            {
                Commit(dialogueTree);
                AssetDatabase.SaveAssets();
                return true;
            }
            return false;
        }

        public bool Validate()
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
        public void Commit(IDialogueTree tree)
        {
            var stack = new Stack<IDialogueNode>();
            stack.Push(root);
            // save new components
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                node.Commit(stack);
            }
            root.PostCommit(tree);
            tree.SharedVariables.Clear();
            foreach (var sharedVariable in SharedVariables)
            {
                tree.SharedVariables.Add(sharedVariable);
            }
            List<GroupBlock> groupBlocks = graphElements.OfType<GroupBlock>().ToList();
            tree.BlockData.Clear();
            foreach (var block in groupBlocks)
            {
                block.Commit(tree.BlockData);
            }
            // Should set tree dirty flag if it is in a prefab
            if (tree is NextGenDialogueTree nextGenDialogueTree)
            {
                EditorUtility.SetDirty(nextGenDialogueTree);
            }
            // notify to unity editor that the tree is changed.
            EditorUtility.SetDirty(tree.Object);
        }
        public string SerializeTreeToJson()
        {
            return DialogueGraphData.Serialize(dialogueTree, false, true);
        }
        public bool CopyFromJson(string serializedData, Vector3 mousePosition)
        {
            var temp = ScriptableObject.CreateInstance<NextGenDialogueTreeAsset>();
            try
            {
                temp.Deserialize(serializedData);
                CopyFromOtherTree(temp, mousePosition);
                return true;
            }
            catch(Exception e)
            {
                Debug.Log(e);
                return false;
            }
        }
    }
}