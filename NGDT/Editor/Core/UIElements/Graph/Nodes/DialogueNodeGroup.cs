using System.Collections.Generic;
using System.Linq;
using Ceres.Editor.Graph;
using Ceres.Graph;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public class DialogueNodeGroup : CeresNodeGroup
    {
        public DialogueNodeGroup()
        {
            AddToClassList(nameof(DialogueNodeGroup));
        }
        
        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            if (element is ModuleNode) return false;
            return true;
        }
        
        public override void Commit(List<NodeGroup> blockData)
        {
            var nodes = containedElements
                                .OfType<IDialogueNode>()
                                .Where(x => x is not ModuleNode)
                                .Select(x => x.Guid).ToList();
            blockData.Add(new NodeGroup
            {
                childNodes = nodes,
                title = title,
                position = GetPosition().position
            });
        }
    }
}
