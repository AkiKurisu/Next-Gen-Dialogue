using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class GroupBlock : Group
    {
        public GroupBlock()
        {
            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));
            capabilities |= Capabilities.Ascendable;
        }
        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (element is ModuleNode) return false;
            return true;
        }
        public void Commit(List<GroupBlockData> blockData)
        {
            var nodes = containedElements
                                .OfType<IDialogueNode>()
                                .Where(x => x is not ModuleNode)
                                .Select(x => x.GUID).ToList();
            blockData.Add(new GroupBlockData
            {
                ChildNodes = nodes,
                Title = title,
                Position = GetPosition().position
            });
        }
        private void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("UnSelect All", (a) =>
            {
                //Clone to prevent self modify
                RemoveElements(containedElements.ToArray());
            }));
        }
    }
}
