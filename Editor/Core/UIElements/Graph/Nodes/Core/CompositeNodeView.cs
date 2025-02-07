using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(CompositeNode), true)]
    public class CompositeNodeView : DialogueNodeView, ILayoutNode
    {
        public bool NoValidate { get; private set; }
        
        public readonly List<Port> ChildPorts = new();

        VisualElement ILayoutNode.View => this;

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Add Child", (a) => AddChild()));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Remove Unnecessary Children", (a) => RemoveUnnecessaryChildren()));
            base.BuildContextualMenu(evt);
        }

        public CompositeNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(CompositeNodeView));
            AddChild();
        }
        
        public void AddChild()
        {
            var child = CreateChildPort();
            ChildPorts.Add(child);
            outputContainer.Add(child);
        }
        
        protected override void Initialize(Type nodeType, DialogueGraphView graphView)
        {
            base.Initialize(nodeType, graphView);
            NoValidate = NodeType.GetCustomAttribute(typeof(NoValidateAttribute), false) != null;
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

        protected override bool OnValidate(Stack<IDialogueNodeView> stack)
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

        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            foreach (var port in ChildPorts)
            {
                if (!port.connections.Any()) continue;
                var child = PortHelper.FindChildNode(port);
                (NodeBehavior as CompositeNode)?.AddChild(child.Compile());
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