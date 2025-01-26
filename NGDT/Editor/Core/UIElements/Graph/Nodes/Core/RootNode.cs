using System;
using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(Root))]
    public sealed class RootNode : DialogueNode, ILayoutNode
    {
        public readonly Port Child;

        private IDialogueNodeView _cache;
        
        public VisualElement View => this;

        public RootNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            title = nameof(Root);
            Child = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(DialoguePort));
            Child.portName = "Child";
            Child.portColor = new Color(191 / 255f, 117 / 255f, 255 / 255f, 0.91f);
            outputContainer.Add(Child);
            capabilities &= ~Capabilities.Copiable;
            capabilities &= ~Capabilities.Deletable;
            capabilities &= ~Capabilities.Movable;
            RefreshExpandedState();
            RefreshPorts();
        }
        
        protected override bool CanAddParent()
        {
            return false;
        }

        protected override void InitializeVisualElements()
        {
            base.InitializeVisualElements();
            DescriptionText.RemoveFromHierarchy();
        }

        protected override void OnRestore()
        {
            ((Root)NodeBehavior).UpdateEditor = ClearStyle;
        }

        protected override bool OnValidate(Stack<IDialogueNodeView> stack)
        {
            // Validate All Pieces and Dialogues
            Graph.CollectNodes<PieceContainer>().ForEach(stack.Push);
            var allDialogues = Graph.CollectNodes<DialogueContainer>();
            allDialogues.ForEach(stack.Push);
            return true;
        }
        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            var newRoot = new Root();
            DialogueContainer child = null;
            
            // Commit main dialogue first
            if (Child.connected)
            {
                child = (DialogueContainer)PortHelper.FindChildNode(Child);
                newRoot.AddChild(child.Compile());
                stack.Push(child);
            }
            else
            {
                // Add empty dialogue
                newRoot.AddChild(new Dialogue());
            }
            
            // Commit all pieces
            Graph.CollectNodes<PieceContainer>()
            .ForEach(x =>
            {
                newRoot.AddChild(x.Compile());
                stack.Push(x);
            });
            
            // Commit left inactive dialogues
            var allDialogues = Graph.CollectNodes<DialogueContainer>();
            if (child != null)
            {
                allDialogues.Remove(child);
            }
            
            allDialogues.ForEach(
                x =>
            {
                newRoot.AddChild(x.Compile());
                stack.Push(x);
            });
            newRoot.UpdateEditor = ClearStyle;
            NodeBehavior = newRoot;
            _cache = child;
        }

        internal void PostCommit(DialogueGraph graph)
        {
            graph.TraverseAppend((Root)NodeBehavior);
        }
        
        protected override void OnClearStyle()
        {
            _cache?.ClearStyle();
            // Clear all dialogue piece
            Graph.CollectNodes<PieceContainer>().ForEach(x => x.ClearStyle());
            if (Child.connected)
            {
                // Clear child dialogue
                PortHelper.FindChildNode(Child).ClearStyle();
            }
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            return new[] { (DialogueContainer)PortHelper.FindChildNode(Child) };
        }
    }
}