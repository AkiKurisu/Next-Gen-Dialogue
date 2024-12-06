using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class ModuleNode : DialogueTreeNode
    {
        public ModuleNode() : base()
        {
            AddToClassList("ModuleNode");
        }
        protected sealed override void AddDescription() { }
        protected override void AddParent()
        {
        }
        protected override bool OnValidate(Stack<IDialogueNode> stack) => true;

        protected override void OnCommit(Stack<IDialogueNode> stack) { }

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
            MapGraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, GetBehavior());
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
    
    public class EditorModuleNode : ModuleNode
    {
        public EditorModuleNode() : base()
        {
            AddToClassList(nameof(EditorModuleNode));
        }
    }
    
    public class BehaviorModuleNode : ModuleNode, ILayoutNode
    {
        private readonly Port _childPort;
        
        public Port Child => _childPort;

        VisualElement ILayoutNode.View => this;

        private IDialogueNode _cache;
        
        public BehaviorModuleNode()
        {
            AddToClassList(nameof(BehaviorModuleNode));
            _childPort = CreateChildPort();
            outputContainer.Add(_childPort);
        }
        
        protected override bool OnValidate(Stack<IDialogueNode> stack)
        {
            if (!_childPort.connected)
            {
                return true;
            }
            stack.Push(PortHelper.FindChildNode(_childPort));
            return true;
        }

        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
            if (!_childPort.connected)
            {
                ((BehaviorModule)NodeBehavior).Child = null;
                _cache = null;
                return;
            }
            var child = PortHelper.FindChildNode(_childPort);
            ((BehaviorModule)NodeBehavior).Child = child.ReplaceBehavior();
            stack.Push(child);
            _cache = child;
        }

        protected override void OnClearStyle()
        {
            _cache?.ClearStyle();
            if (_childPort.connected)
            {
                PortHelper.FindChildNode(_childPort).ClearStyle();
            }
        }

        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            var list = new List<ILayoutNode>();
            if (_childPort.connected) list.Add((ILayoutNode)PortHelper.FindChildNode(_childPort));
            return list;
        }
    }
}
