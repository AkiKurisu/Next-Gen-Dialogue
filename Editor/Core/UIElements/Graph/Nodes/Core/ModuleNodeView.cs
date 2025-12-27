using System;
using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEngine;
using UnityEngine.UIElements;

namespace NextGenDialogue.Graph.Editor
{
    [CustomNodeView(typeof(Module), true)]
    public class ModuleNodeView : DialogueNodeView
    {
        public ModuleNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(ModuleNodeView));
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
            GraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, NodeType);
        }
        
        public sealed override Rect GetWorldPosition()
        {
            ContainerNodeView parentContainer= GetFirstAncestorOfType<ContainerNodeView>();
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
    public class EditorModuleNodeView : ModuleNodeView
    {
        public EditorModuleNodeView(Type type, CeresGraphView graphView): base(type, graphView)
        {
            AddToClassList(nameof(EditorModuleNodeView));
        }
    }
}
