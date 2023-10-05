using System.Collections;
namespace Kurisu.NGDS
{
    public interface IDialogueResolver
    {
        IEnumerator EnterDialogue();
        void ExitDialogue();
        Dialogue Dialogue { get; }
        void Inject(Dialogue dialogue, IDialogueSystem system);
    }
}
