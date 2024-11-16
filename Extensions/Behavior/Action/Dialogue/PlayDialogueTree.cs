using Ceres;
using Ceres.Annotations;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Play another dialogue tree")]
    [NodeLabel("Dialogue: Play DialogueTree")]
    [NodeGroup("Dialogue")]
    public class PlayDialogueTree : Action
    {
        public SharedTObject<NextGenDialogueComponent> dialogueTree;
        protected override Status OnUpdate()
        {
            if (dialogueTree.Value) dialogueTree.Value.GetDialogueGraph().PlayDialogue(dialogueTree.Value.gameObject);
            return Status.Success;
        }
    }
}
