using System;
using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [CustomNodeView(typeof(Module), true)]
    public class ModuleNode : DialogueNode
    {
        public ModuleNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(ModuleNode));
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
        
        protected override bool OnValidate(Stack<IDialogueNodeView> stack) => true;

        protected override void OnCommit(Stack<IDialogueNodeView> stack) { }

        protected override void OnClearStyle() { }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                return e switch
                {
                    CeresDropdownMenuAction a => false,
                    DropdownMenuAction a => a.name == "Create Node" || a.name == "Delete",
                    _ => false,
                };
            });
            //Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
            Graph.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
        }
        
        protected override void OnGeometryChanged(GeometryChangedEvent evt)
        {
            bool isAttached = GetFirstAncestorOfType<ContainerNode>() != null;
            if (SettingButton != null && SettingsContainer != null && SettingsContainer.parent != null)
            {
                var settingsButtonLayout = SettingButton.ChangeCoordinatesTo(SettingsContainer.parent, SettingButton.layout);
                SettingsContainer.style.top = settingsButtonLayout.yMax - (isAttached ? 70f : 20f);
                SettingsContainer.style.left = settingsButtonLayout.xMin - layout.width + (isAttached ? 10f : 20f);
            }
        }
        
        public sealed override Rect GetWorldPosition()
        {
            ContainerNode parentContainer= GetFirstAncestorOfType<ContainerNode>();
            var rect = GetPosition();
            if (parentContainer != null)
            {
                var parentRect = parentContainer.GetPosition();
                rect.x += parentRect.x;
                rect.y += parentRect.y;
            }
            return rect;
        }
    }
    
    [CustomNodeView(typeof(EditorModule), true)]
    public class EditorModuleNode : ModuleNode
    {
        public EditorModuleNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(EditorModuleNode));
        }
    }
    
    [CustomNodeView(typeof(BehaviorModule), true)]
    public class BehaviorModuleNode : ModuleNode, ILayoutNode
    {
        public Port Child { get; }

        VisualElement ILayoutNode.View => this;

        private IDialogueNodeView _cache;
        
        public BehaviorModuleNode(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(BehaviorModuleNode));
            Child = CreateChildPort();
            outputContainer.Add(Child);
        }
        
        protected override bool OnValidate(Stack<IDialogueNodeView> stack)
        {
            if (!Child.connected)
            {
                return true;
            }
            stack.Push(PortHelper.FindChildNode(Child));
            return true;
        }

        protected override void OnCommit(Stack<IDialogueNodeView> stack)
        {
            if (!Child.connected)
            {
                ((BehaviorModule)NodeBehavior).Child = null;
                _cache = null;
                return;
            }
            var child = PortHelper.FindChildNode(Child);
            ((BehaviorModule)NodeBehavior).Child = child.Compile();
            stack.Push(child);
            _cache = child;
        }

        protected override void OnClearStyle()
        {
            _cache?.ClearStyle();
            if (Child.connected)
            {
                PortHelper.FindChildNode(Child).ClearStyle();
            }
        }

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (Child.connected) list.Add((ILayoutNode)PortHelper.FindChildNode(Child));
            return list;
        }
    }
}
