using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class ConditionalNode : DialogueTreeNode
    {
        private readonly Port childPort;

        public Port Child => childPort;

        private IDialogueNode cache;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ConditionalSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSetting.GetMask());
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
                (NodeBehavior as Conditional).Child = null;
                cache = null;
                return;
            }
            var child = childPort.connections.First().input.node as IDialogueNode;
            (NodeBehavior as Conditional).Child = child.ReplaceBehavior();
            stack.Push(child);
            cache = child;
        }

        protected override void OnClearStyle()
        {
            cache?.ClearStyle();
        }
    }
}