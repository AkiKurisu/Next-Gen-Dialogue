using System.Collections.Generic;
using System.Linq;
using Ceres.Editor.Graph;
using Ceres.Graph;
using UnityEditor.Experimental.GraphView;

namespace NextGenDialogue.Graph.Editor
{
    public class DialogueNodeGroup : CeresNodeGroup
    {
        public DialogueNodeGroup()
        {
            AddToClassList(nameof(DialogueNodeGroup));
        }
        
        public override bool AcceptsElement(GraphElement element, ref string reasonWhyNotAccepted)
        {
            return element is not ModuleNodeView;
        }
        
        public override void Commit(List<NodeGroup> nodeGroups)
        {
            var nodes = containedElements
                                .OfType<IDialogueNodeView>()
                                .Where(x => x is not ModuleNodeView)
                                .Select(x => x.Guid)
                                .ToList();
            nodeGroups.Add(new NodeGroup
            {
                childNodes = nodes,
                title = title,
                position = GetPosition().position
            });
        }
    }
}
