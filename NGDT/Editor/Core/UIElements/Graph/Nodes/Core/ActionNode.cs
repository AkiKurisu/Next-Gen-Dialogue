using System;
using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(Action), true)]
    public class ActionNode : DialogueNode, ILayoutNode
    {
        VisualElement ILayoutNode.View => this;

        public ActionNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(ActionNode));
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