using System;
using Ceres.Editor;
using Ceres.Editor.Graph;
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

        protected override void OnClearStyle() { }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);
            var remainTargets = evt.menu.MenuItems().FindAll(e =>
            {
                return e switch
                {
                    CeresDropdownMenuAction => false,
                    DropdownMenuAction a => a.name is "Create Node" or "Delete",
                    _ => false,
                };
            });
            // Remove needless default actions .
            evt.menu.MenuItems().Clear();
            remainTargets.ForEach(evt.menu.MenuItems().Add);
            GraphView.ContextualMenuRegistry.BuildContextualMenu(ContextualMenuType.Node, evt, NodeType);
        }

        protected override void OnSerialize()
        {
            var parentNodeView = GetFirstAncestorOfType<ContainerNodeView>();
            if (parentNodeView != null)
            {
                // Use parent position instead
                NodeInstance.NodeData.graphPosition = parentNodeView.GetPosition();
            }
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
