using System.Linq;
using Ceres.Editor.Graph;
using UnityEngine;
using UNode = UnityEditor.Experimental.GraphView.Node;

namespace NextGenDialogue.Graph.Editor
{
    public class DialogueNodeGroupHandler : NodeGroupHandler<DialogueNodeGroup>
    {
        public DialogueNodeGroupHandler(DialogueGraphView graphView) : base(graphView)
        {
        }
        
        public override void DoGroup()
        {
            var nodes = GraphView.selection.OfType<UNode>()
                                        .Where(x=>x is IDialogueNodeView and not RootNodeView)
                                        .ToArray();
            if(!nodes.Any()) return;
#if UNITY_6000_0_OR_NEWER
            var rect = new Rect(nodes[0].resolvedStyle.translate, new Vector2(100, 100));
#else
            var rect = new Rect(nodes[0].transform.position, new Vector2(100, 100));
#endif
            var block = CreateGroup(rect);
            foreach (var node in nodes)
            {
                block.AddElement(node);
            }
        }
        
        public override void DoUnGroup()
        {
            foreach (var select in GraphView.selection)
            {
                if (select is not IDialogueNodeView) continue;
                var node = select as UNode;
                var block = GraphView.graphElements.OfType<DialogueNodeGroup>().FirstOrDefault(nodeGroup => nodeGroup.ContainsElement(node));
                block?.RemoveElement(node);
            }
        }
    }
}