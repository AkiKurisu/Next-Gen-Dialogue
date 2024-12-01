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
namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeView : CeresGraphView
    {
        public IDialogueContainer DialogueContainer { get; }

        private RootNode _root;
        
        // ReSharper disable once InconsistentNaming
        public Action<IDialogueNode> OnSelectNode;
        
        private readonly NodeConvertor _converter = new();
        
        public DialogueTreeView(CeresGraphEditorWindow editorWindow) : base(editorWindow)
        {
            DialogueContainer = (IDialogueContainer)editorWindow.Container;
            styleSheets.Add(NextGenDialogueSetting.GetGraphStyle());
            AddSearchWindow<DialogueNodeSearchWindow>();
            AddGroupBlockHandler(new DialogueGroupBlockHandler(this));
        }

        public override void OpenSearch(Vector2 screenPosition)
        {
            /* Override context from settings */
            SearchWindow.Initialize(this, NextGenDialogueSetting.GetNodeSearchContext());
            USearchWindow.Open(new SearchWindowContext(screenPosition), SearchWindow);
        }

        public override void OpenSearch(Vector2 screenPosition, CeresPortView portView)
        {
            /* Port not support in dialogue graph view */
            OpenSearch(screenPosition);
        }

        protected override string OnSerialize(IEnumerable<GraphElement> elements)
        {
            CopyPaste.Copy(EditorWindow.GetInstanceID(), elements.ToArray());
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
            Rect newRect = node.GetWorldPosition();
            newRect.position += new Vector2(50, 50);
            nodeElement!.SetPosition(newRect);
            newNode.OnSelect = OnSelectNode;
            AddElement(nodeElement);
            newNode.CopyFrom(node);
            return newNode;
        }

        public override void AddNodeView(ICeresNodeView nodeView)
        {
            var dialogueNode = (IDialogueNode)nodeView;
            AddElement(dialogueNode.NodeElement);
            dialogueNode.OnSelect = OnSelectNode;
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
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Paste", evt =>
            {
                Paste(contentViewContainer.WorldToLocal(evt.eventInfo.mousePosition) - CopyPaste.CenterPosition);
            }, x => CopyPaste.CanPaste ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled));
            ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Graph, evt, null);
        }
        public sealed override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return PortHelper.GetCompatiblePorts(this, startAnchor);
        }
        protected override void OnDragDropObjectPerform(UnityEngine.Object data, Vector3 mousePosition)
        {
            if (data is GameObject)
            {
                if ((data as GameObject).TryGetComponent(out IDialogueContainer tree))
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
            if (data is not IDialogueContainer)
            {
                EditorWindow.ShowNotification(new GUIContent("Invalid Drag Data !"));
                return;
            }
            EditorWindow.ShowNotification(new GUIContent("Data Dropped Succeed !"));
            CopyFromOtherTree(data as IDialogueContainer, mousePosition);
        }
        public void CopyFromOtherTree(IDialogueContainer otherTree, Vector2 mousePosition)
        {
            var localMousePosition = contentViewContainer.WorldToLocal(mousePosition) - new Vector2(400, 300);
            foreach (var variable in otherTree.SharedVariables)
            {
                Blackboard.AddVariable(variable.Clone(), false);
            }
            var (rootNode, newNodes) = _converter.ConvertToNode(otherTree, this, localMousePosition);
            var edge = rootNode.Child.connections.First();
            RemoveElement(edge);
            RemoveElement(rootNode);
            RestoreBlocks(otherTree, newNodes);
        }
        public void Restore()
        {
            if (DialogueTreeEditorUtility.TryGetExternalTree(DialogueContainer, out IDialogueContainer tree))
            {
                OnRestore(tree);
            }
            else
            {
                OnRestore(DialogueContainer);
            }
        }
        private void OnRestore(IDialogueContainer tree)
        {
            // Add default dialogue
            if (tree.Root.Child == null)
            {
                tree.Root.Child = new Dialogue();
                var pos = tree.Root.NodeData.graphPosition;
                pos.x += 200;
                tree.Root.Child.NodeData.graphPosition = pos;
            }
            AddSharedVariables(tree.SharedVariables,true);
            List<IDialogueNode> newNodes;
            (_root, newNodes) = _converter.ConvertToNode(tree, this, Vector2.zero);
            RestoreBlocks(tree, newNodes);
        }
        private void RestoreBlocks(IDialogueContainer tree, List<IDialogueNode> inNodes)
        {
            foreach (var nodeBlockData in tree.BlockData)
            {
                GroupBlockHandler.CreateGroup(new Rect(nodeBlockData.position, new Vector2(100, 100)), nodeBlockData)
                .AddElements(inNodes.Where(x => nodeBlockData.childNodes.Contains(x.Guid)).Cast<Node>());
            }
        }

        public bool Save()
        {
            if (Application.isPlaying) return false;
            if (!Validate()) return false;
            
            Commit(DialogueContainer);
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
        public void Commit(IDialogueContainer tree)
        {
            var stack = new Stack<IDialogueNode>();
            stack.Push(_root);
            // save new components
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                node.Commit(stack);
            }
            _root.PostCommit(tree);
            tree.SharedVariables.Clear();
            foreach (var sharedVariable in SharedVariables)
            {
                tree.SharedVariables.Add(sharedVariable);
            }
            List<DialogueGroup> groupBlocks = graphElements.OfType<DialogueGroup>().ToList();
            tree.BlockData.Clear();
            foreach (var block in groupBlocks)
            {
                block.Commit(tree.BlockData);
            }
            // Should set tree dirty flag if it is in a prefab
            if (tree is NextGenDialogueComponent nextGenDialogueTree)
            {
                EditorUtility.SetDirty(nextGenDialogueTree);
            }
            // notify to unity editor that the tree is changed.
            EditorUtility.SetDirty(tree.Object);
        }
        public string SerializeTreeToJson()
        {
            return DialogueGraphData.Serialize(DialogueContainer, false, true);
        }
        public bool CopyFromJson(string serializedData, Vector3 mousePosition)
        {
            var temp = ScriptableObject.CreateInstance<NextGenDialogueAsset>();
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