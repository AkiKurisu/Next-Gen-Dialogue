namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Play another dialogue tree")]
    [AkiLabel("Dialogue: Play DialogueTree")]
    [AkiGroup("Dialogue")]
    public class PlayDialogueTree : Action
    {
        public SharedTObject<NextGenDialogueTree> dialogueTree;
        protected override Status OnUpdate()
        {
            if (dialogueTree.Value != null) dialogueTree.Value.PlayDialogue();
            return Status.Success;
        }
    }
}
