using System.Collections.Generic;
using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class ConditionalNode : DialogueNode, ILayoutNode
    {
        private readonly Port childPort;

        public Port Child => childPort;

        VisualElement ILayoutNode.View => this;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ConditionalSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSettings.GetNodeSearchContext());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
            base.BuildContextualMenu(evt);
        }

        public ConditionalNode()
        {
            AddToClassList("ConditionalNode");
            childPort = CreateChildPort();
            outputContainer.Add(childPort);
        }

        protected override bool OnValidate(Stack<IDialogueNode> stack)
        {
            if (!childPort.connected)
            {
                return true;
            }
            stack.Push(childPort.connections.First().input.node as IDialogueNode);
            return true;
        }

        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
            if (!childPort.connected)
            {
                ((Conditional)NodeBehavior).Child = null;
                return;
            }
            var child = PortHelper.FindChildNode(childPort);
            ((Conditional)NodeBehavior).Child = child.Compile();
            stack.Push(child);
        }

        protected override void OnClearStyle()
        {
            if (!childPort.connected) return;
            var child = PortHelper.FindChildNode(childPort);
            child.ClearStyle();
        }
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (childPort.connected) list.Add((ILayoutNode)PortHelper.FindChildNode(childPort));
            return list;
        }
    }
}