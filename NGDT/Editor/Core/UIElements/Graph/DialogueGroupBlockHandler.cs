using System.Linq;
using Ceres.Editor.Graph;
using Ceres.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class DialogueGroupBlockHandler : GroupBlockHandler
    {
        public DialogueGroupBlockHandler(DialogueGraphView graphView) : base(graphView)
        {
        }
        
        public override Group CreateGroup(Rect rect, NodeGroupBlock blockData = null)
        {
            blockData ??= new NodeGroupBlock();
            var group = new DialogueGroup
            {
                autoUpdateGeometry = true,
                title = blockData.title
            };
            GraphView.AddElement(group);
            group.SetPosition(rect);
            return group;
        }
        public override void SelectGroup(Node node)
        {
            var block = CreateGroup(new Rect(node.transform.position, new Vector2(100, 100)));
            foreach (var select in GraphView.selection)
            {
                if (select is not IDialogueNode or RootNode) continue;
                block.AddElement(select as Node);
            }
        }
        public override void UnselectGroup()
        {
            foreach (var select in GraphView.selection)
            {
                if (select is not IDialogueNode) continue;
                var node = select as Node;
                var block = GraphView.graphElements.OfType<DialogueGroup>().FirstOrDefault(x => x.ContainsElement(node));
                block?.RemoveElement(node);
            }
        }
    }
}