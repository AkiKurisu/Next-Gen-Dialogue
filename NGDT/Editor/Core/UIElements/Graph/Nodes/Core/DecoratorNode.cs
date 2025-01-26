using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(Decorator), true)]
    public class DecoratorNode : DialogueNode, ILayoutNode
    {
        public Port Child { get; }

        VisualElement ILayoutNode.View => this;

        public DecoratorNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(DecoratorNode));
            Child = CreateChildPort();
            outputContainer.Add(Child);
        }

        protected override bool OnValidate(Stack<IDialogueNodeView> stack)
        {
            if (!Child.connected)
            {
                return false;
            }
            stack.Push(Child.connections.First().input.node as DialogueNode);
            return true;
        }

        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            if (!Child.connected)
            {
                ((Decorator)NodeBehavior).Child = null;
                return;
            }
            var child = PortHelper.FindChildNode(Child);
            ((Decorator)NodeBehavior).Child = child.Compile();
            stack.Push(child);
        }

        protected override void OnClearStyle()
        {
            if (!Child.connected) return;
            var child = PortHelper.FindChildNode(Child);
            child.ClearStyle();
        }

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (Child.connected) list.Add((ILayoutNode)PortHelper.FindChildNode(Child));
            return list;
        }
    }
}