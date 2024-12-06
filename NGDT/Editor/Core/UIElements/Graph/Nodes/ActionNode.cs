using System.Collections.Generic;
using Ceres.Editor;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class ActionNode : DialogueTreeNode, ILayoutNode
    {
        VisualElement ILayoutNode.View => this;

        public ActionNode()
        {
            AddToClassList("ActionNode");
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ActionSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSetting.GetNodeSearchContext());
                SearchWindow.Open(new SearchWindowContext(a.eventInfo.localMousePosition), provider);
            }));
            base.BuildContextualMenu(evt);
        }

        protected override bool OnValidate(Stack<IDialogueNode> stack) => true;

        protected override void OnCommit(Stack<IDialogueNode> stack)
        {
        }

        protected override void OnClearStyle()
        {
        }
        public IReadOnlyList<ILayoutNode> GetLayoutChildren()
        {
            return new List<ILayoutNode>();
        }
    }
}