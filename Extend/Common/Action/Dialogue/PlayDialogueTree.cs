using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action : Play another dialogue tree")]
    [AkiLabel("Dialogue:PlayDialogueTree")]
    [AkiGroup("Dialogue")]
    public class PlayDialogueTree : Action
    {
        [SerializeField]
        private SharedTObject<NextGenDialogueTree> dialogueTree;
        public override void Awake()
        {
            InitVariable(dialogueTree);
        }
        protected override Status OnUpdate()
        {
            dialogueTree.Value?.PlayDialogue();
            return Status.Success;
        }
    }
}
