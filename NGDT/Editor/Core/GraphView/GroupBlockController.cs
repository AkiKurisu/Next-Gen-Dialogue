using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class GroupBlockController : IControlGroupBlock
    {
        private readonly GraphView graphView;
        public GroupBlockController(GraphView graphView)
        {
            this.graphView = graphView;
        }
        public GroupBlock CreateBlock(Rect rect, GroupBlockData blockData = null)
        {
            blockData ??= new GroupBlockData();
            var group = new GroupBlock
            {
                autoUpdateGeometry = true,
                title = blockData.Title
            };
            graphView.AddElement(group);
            group.SetPosition(rect);
            return group;
        }
        public void SelectGroup(IDialogueNode node)
        {
            var block = CreateBlock(new Rect((node as Node).transform.position, new Vector2(100, 100)));
            foreach (var select in graphView.selection)
            {
                if (select is not IDialogueNode || select is RootNode) continue;
                block.AddElement(select as Node);
            }
        }
        public void UnSelectGroup()
        {
            foreach (var select in graphView.selection)
            {
                if (select is not IDialogueNode) continue;
                var node = select as Node;
                var block = graphView.graphElements.OfType<GroupBlock>().FirstOrDefault(x => x.ContainsElement(node));
                block?.RemoveElement(node);
            }
        }
    }
}
