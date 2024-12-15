using Ceres;
using Ceres.Annotations;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Play another dialogue tree")]
    [CeresLabel("Dialogue: Play DialogueTree")]
    [NodeGroup("Dialogue")]
    public class PlayDialogueTree : Action
    {
        public SharedUObject<NextGenDialogueComponent> dialogueTree;
        protected override Status OnUpdate()
        {
            if (dialogueTree.Value) dialogueTree.Value.GetDialogueGraph().PlayDialogue(dialogueTree.Value.gameObject);
            return Status.Success;
        }
    }
}
