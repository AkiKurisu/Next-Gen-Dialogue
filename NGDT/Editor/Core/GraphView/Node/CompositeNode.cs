using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class CompositeNode : DialogueTreeNode
    {
        public readonly List<Port> ChildPorts = new();
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<CompositeSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSetting.GetMask());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Add Child", (a) => AddChild()));
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Remove Unnecessary Children", (a) => RemoveUnnecessaryChildren()));
            base.BuildContextualMenu(evt);
        }

        public CompositeNode()
        {
            AddToClassList("CompositeNode");
            AddChild();
        }
        public void AddChild()
        {
            var child = CreateChildPort();
            ChildPorts.Add(child);
            outputContainer.Add(child);
        }

        private void RemoveUnnecessaryChildren()
        {
            var unnecessary = ChildPorts.Where(p => !p.connected).ToList();
            unnecessary.ForEach(e =>
            {
                ChildPorts.Remove(e);
                outputContainer.Remove(e);
            });
        }

        protected override bool OnValidate(Stack<IDialogueNode> stack)
        {
            if (ChildPorts.Count <= 0) return false;
            foreach (var port in ChildPorts)
            {
                if (!port.connected)
                {
                    style.backgroundColor = Color.red;
                    return false;
                }
                stack.Push(PortHelper.FindChildNode(port));
            }
            style.backgroundColor = new StyleColor(StyleKeyword.Null);
            return true;
        }

        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
            foreach (var port in ChildPorts)
            {
                if (port.connections.Count() == 0) continue;
                var child = PortHelper.FindChildNode(port);
                (NodeBehavior as Composite).AddChild(child.ReplaceBehavior());
                stack.Push(child);
            }
        }

        protected override void OnClearStyle()
        {
            foreach (var port in ChildPorts)
            {
                if (port.connections.Count() == 0) continue;
                var child = PortHelper.FindChildNode(port);
                child.ClearStyle();
            }
        }
    }
}