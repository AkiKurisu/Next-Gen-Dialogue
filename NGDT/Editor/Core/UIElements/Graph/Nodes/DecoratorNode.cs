using System.Collections.Generic;
using System.Linq;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class DecoratorNode : DialogueNode, ILayoutNode
    {
        private readonly Port childPort;

        public Port Child => childPort;

        VisualElement ILayoutNode.View => this;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<DecoratorSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSettings.GetNodeSearchContext());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
            base.BuildContextualMenu(evt);
        }

        public DecoratorNode()
        {
            AddToClassList("DecoratorNode");
            childPort = CreateChildPort();
            outputContainer.Add(childPort);
        }

        protected override bool OnValidate(Stack<IDialogueNode> stack)
        {
            if (!childPort.connected)
            {
                return false;
            }
            stack.Push(childPort.connections.First().input.node as DialogueNode);
            return true;
        }

        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
            if (!childPort.connected)
            {
                ((Decorator)NodeBehavior).Child = null;
                return;
            }
            var child = PortHelper.FindChildNode(childPort);
            ((Decorator)NodeBehavior).Child = child.Compile();
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