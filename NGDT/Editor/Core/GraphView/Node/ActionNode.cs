using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class ActionNode : DialogueTreeNode
    {
        public ActionNode()
        {
            AddToClassList("ActionNode");
        }
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Change Behavior", (a) =>
            {
                var provider = ScriptableObject.CreateInstance<ActionSearchWindowProvider>();
                provider.Init(this, NextGenDialogueSetting.GetMask());
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
    }
}