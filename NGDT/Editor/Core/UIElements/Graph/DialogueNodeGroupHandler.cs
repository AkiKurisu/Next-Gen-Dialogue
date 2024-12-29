using System.Linq;
using Ceres.Editor.Graph;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class DialogueNodeGroupHandler : NodeGroupHandler<DialogueNodeGroup>
    {
        public DialogueNodeGroupHandler(DialogueGraphView graphView) : base(graphView)
        {
        }
        
        public override void DoGroup()
        {
            var nodes = GraphView.selection.OfType<Node>()
                                        .Where(x=>x is IDialogueNode and not RootNode)
                                        .ToArray();
            if(!nodes.Any()) return;
            var block = CreateGroup(new Rect(nodes[0].transform.position, new Vector2(100, 100)));
            foreach (var node in nodes)
            {
                block.AddElement(node);
            }
        }
        
        public override void DoUnGroup()
        {
            foreach (var select in GraphView.selection)
            {
                if (select is not IDialogueNode) continue;
                var node = select as Node;
                var block = GraphView.graphElements.OfType<DialogueNodeGroup>().FirstOrDefault(x => x.ContainsElement(node));
                block?.RemoveElement(node);
            }
        }
    }
}