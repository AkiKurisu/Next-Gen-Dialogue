using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System;
using Ceres.Editor;
namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeView : CeresGraphView
    {
        private readonly IDialogueContainer _dialogueTree;
        public IDialogueContainer DialogueTree => _dialogueTree;
        
        private RootNode _root;

        private readonly NodeResolverFactory _nodeResolver = NodeResolverFactory.Instance;
        public Action<IDialogueNode> OnSelectNode { get; internal set; }
        
        private readonly NodeConvertor _converter = new();
        
        public DialogueTreeView(IDialogueContainer bt, EditorWindow editorWindow) : base(editorWindow)
        {
            _dialogueTree = bt;
            styleSheets.Add(NextGenDialogueSetting.GetGraphStyle());
            var provider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();
            provider.Initialize(this, NextGenDialogueSetting.GetMask());
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
            };
            GroupBlockHandler = new DialogueGroupBlockHandler(this);
        }

        protected override string OnSerialize(IEnumerable<GraphElement> elements)
        {
            CopyPaste.Copy(EditorWindow.GetInstanceID(), elements);
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
            //Add paste elements to selection
            foreach (var element in new CopyPasteGraphConvertor(this, CopyPaste.Paste(), positionOffSet).GetCopyElements())
            {
                element.Select(this, true);
            }
        }
        public IDialogueNode DuplicateNode(IDialogueNode node)
        {
            var newNode = _nodeResolver.Create(node.GetBehavior(), this);
            if (newNode is PieceContainer pieceContainer)
            {
                pieceContainer.GenerateNewPieceID();
            }
            var nodeElement = newNode as Node;
            Rect newRect = node.GetWorldPosition();
            newRect.position += new Vector2(50, 50);
            nodeElement.SetPosition(newRect);
            newNode.OnSelect = OnSelectNode;
            AddElement(nodeElement);
            newNode.CopyFrom(node);
            return newNode;
        }
        public void AddNode(IDialogueNode node, Rect worldRect)
        {
            node.View.SetPosition(worldRect);
            AddElement(node.View);
            node.OnSelect = OnSelectNode;
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
            ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Graph, evt, null);
        }
        public sealed override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return PortHelper.GetCompatiblePorts(this, startAnchor);
        }
        protected override void CopyFromObject(UnityEngine.Object data, Vector3 mousePosition)
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
            IEnumerable<IDialogueNode> nodes;
            RootNode rootNode;
            foreach (var variable in otherTree.SharedVariables)
            {
                Blackboard.AddVariable(variable.Clone(), false);
            }
            (rootNode, nodes) = _converter.ConvertToNode(otherTree, this, localMousePosition);
            foreach (var node in nodes) node.OnSelect = OnSelectNode;
            var edge = rootNode.Child.connections.First();
            RemoveElement(edge);
            RemoveElement(rootNode);
            RestoreBlocks(otherTree, nodes);
        }
        public void Restore()
        {
            if (DialogueTreeEditorUtility.TryGetExternalTree(DialogueTree, out IDialogueContainer tree))
            {
                OnRestore(tree);
            }
            else
            {
                OnRestore(DialogueTree);
            }
        }
        private void OnRestore(IDialogueContainer tree)
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
            (_root, nodes) = _converter.ConvertToNode(tree, this, Vector2.zero);
            foreach (var node in nodes) node.OnSelect = OnSelectNode;
            RestoreBlocks(tree, nodes);
        }
        private void RestoreSharedVariables(IDialogueContainer tree)
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
        private void RestoreBlocks(IDialogueContainer tree, IEnumerable<IDialogueNode> nodes)
        {
            foreach (var nodeBlockData in tree.BlockData)
            {
                GroupBlockHandler.CreateGroup(new Rect(nodeBlockData.Position, new Vector2(100, 100)), nodeBlockData)
                .AddElements(nodes.Where(x => nodeBlockData.ChildNodes.Contains(x.GUID)).Cast<Node>());
            }
        }

        public bool Save()
        {
            if (Application.isPlaying) return false;
            if (Validate())
            {
                Commit(_dialogueTree);
                AssetDatabase.SaveAssets();
                return true;
            }
            return false;
        }

        public bool Validate()
        {
            //validate nodes by DFS.
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
            return DialogueGraphData.Serialize(_dialogueTree, false, true);
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