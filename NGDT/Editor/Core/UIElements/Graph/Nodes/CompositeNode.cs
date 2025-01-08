using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class CompositeNode : DialogueNode, ILayoutNode
    {
        public bool NoValidate { get; private set; }
        
        public readonly List<Port> ChildPorts = new();

        VisualElement ILayoutNode.View => this;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<CompositeSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSettings.GetNodeSearchContext());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Child", (a) => AddChild()));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Remove Unnecessary Children", (a) => RemoveUnnecessaryChildren()));
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
        
        protected override void OnBehaviorSet()
        {
            NoValidate = GetBehavior().GetCustomAttribute(typeof(NoValidateAttribute), false) != null;
        }
        
        public void RemoveUnnecessaryChildren()
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
            if (ChildPorts.Count <= 0 && !NoValidate) return false;
            foreach (var port in ChildPorts)
            {
                if (!port.connected)
                {
                    if (NoValidate) continue;
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
                if (!port.connections.Any()) continue;
                var child = PortHelper.FindChildNode(port);
                (NodeBehavior as Composite)?.AddChild(child.Compile());
                stack.Push(child);
            }
        }

        protected override void OnClearStyle()
        {
            foreach (var port in ChildPorts)
            {
                if (!port.connections.Any()) continue;
                var child = PortHelper.FindChildNode(port);
                child.ClearStyle();
            }
        }

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            foreach (var port in ChildPorts)
            {
                if (!port.connections.Any()) continue;
                list.Add((ILayoutNode)PortHelper.FindChildNode(port));
            }
            list.Reverse();
            return list;
        }
    }
}