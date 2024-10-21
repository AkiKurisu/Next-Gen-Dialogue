namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Play another dialogue tree")]
    [NodeLabel("Dialogue: Play DialogueTree")]
    [NodeGroup("Dialogue")]
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
