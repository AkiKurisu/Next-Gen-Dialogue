using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(Root))]
    public sealed class RootNode : DialogueTreeNode, ILayoutNode
    {
        public readonly Port Child;

        private IDialogueNode _cache;
        
        public VisualElement View => this;

        public RootNode()
        {
            SetBehavior(typeof(Root));
            title = nameof(Root);
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
            ((Root)NodeBehavior).UpdateEditor = ClearStyle;
        }

        protected sealed override bool OnValidate(Stack<IDialogueNode> stack)
        {
            //Validate All Pieces and Dialogues
            MapGraphView.CollectNodes<PieceContainer>()
            .ForEach(x =>
            {
                stack.Push(x);
            });
            var allDialogues = MapGraphView.CollectNodes<DialogueContainer>();
            allDialogues.ForEach(x => stack.Push(x));
            return true;
        }
        protected sealed override void OnCommit(Stack<IDialogueNode> stack)
        {
            var newRoot = new Root();
            DialogueContainer child = null;
            // Commit Dialogue Piece First, no matter Piece is linked to Dialogue, they will be committed
            MapGraphView.CollectNodes<PieceContainer>()
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
            var allDialogues = MapGraphView.CollectNodes<DialogueContainer>();
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
            _cache = child;
        }

        public void PostCommit(IDialogueContainer tree)
        {
            DialogueContainerUtility.SetRoot(tree, NodeBehavior as Root);
        }
        protected sealed override void OnClearStyle()
        {
            _cache?.ClearStyle();
            //Clear all dialogue piece
            MapGraphView.CollectNodes<PieceContainer>().ForEach(x => x.ClearStyle());
            if (Child.connected)
            {
                //Clear child dialogue
                PortHelper.FindChildNode(Child).ClearStyle();
            }
        }
        public sealed override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            return new[] { (DialogueContainer)PortHelper.FindChildNode(Child) };
        }
    }
}