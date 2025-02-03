using System;
using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(ActionNode), true)]
    public class ActionNodeViewView : DialogueNodeView, ILayoutNode
    {
        VisualElement ILayoutNode.View => this;

        public ActionNodeViewView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(ActionNodeViewView));
        }

        protected override bool OnValidate(Stack<IDialogueNodeView> stack) => true;

        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
        }

        protected override void OnClearStyle()
        {
        }
        
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            return new List<ILayoutNode>();
        }
    }
}