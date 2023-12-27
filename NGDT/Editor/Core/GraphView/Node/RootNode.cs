using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public sealed class RootNode : DialogueTreeNode
    {
        public readonly Port Child;

        private IDialogueNode cache;

        public RootNode()
        {
            SetBehavior(typeof(Root));
            title = "Root";
            Child = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialoguePort));
            Child.portName = "Child";
            Child.portColor = new UnityEngine.Color(191 / 255f, 117 / 255f, 255 / 255f, 0.91f);
            outputContainer.Add(Child);
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            RefreshExpandedState();
            RefreshPorts();
        }

        protected sealed override void AddParent() { }

        protected sealed override void AddDescription() { }

        protected sealed override void OnRestore()
        {
            (NodeBehavior as Root).UpdateEditor = ClearStyle;
        }

        protected sealed override bool OnValidate(Stack<IDialogueNode> stack)
        {
            //Validate All Pieces and Dialogues
            MapTreeView.CollectNodes<PieceContainer>()
            .ForEach(x =>
            {
                stack.Push(x);
            });
            var allDialogues = MapTreeView.CollectNodes<DialogueContainer>();
            allDialogues.ForEach(x => stack.Push(x));
            return true;
        }
        protected sealed override void OnCommit(Stack<IDialogueNode> stack)
        {
            var newRoot = new Root();
            DialogueContainer child = null;
            //Commit Dialogue Piece First, no matter Piece is linked to Dialogue, they will be committed
            MapTreeView.CollectNodes<PieceContainer>()
            .ForEach(x =>
            {
                newRoot.AddChild(x.ReplaceBehavior());
                stack.Push(x);
            });
            if (Child.connected)
            {
                child = (DialogueContainer)PortHelper.FindChildNode(Child);
                newRoot.Child = child.ReplaceBehavior();
                stack.Push(child);
            }
            var allDialogues = MapTreeView.CollectNodes<DialogueContainer>();
            if (child != null)
                allDialogues.Remove(child);
            allDialogues.ForEach(
                x =>
            {
                newRoot.AddChild(x.ReplaceBehavior());
                stack.Push(x);
            });
            newRoot.UpdateEditor = ClearStyle;
            NodeBehavior = newRoot;
            cache = child;
        }

        public void PostCommit(IDialogueTree tree)
        {
            DialogueTreeEditorUtility.SetRoot(tree, NodeBehavior as Root);
        }
        protected sealed override void OnClearStyle()
        {
            cache?.ClearStyle();
            //Clear all dialogue piece
            MapTreeView.CollectNodes<PieceContainer>().ForEach(x => x.ClearStyle());
            if (Child.connected)
            {
                //Clear child dialogue
                PortHelper.FindChildNode(Child).ClearStyle();
            }
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
    }
}