using System.Collections;
namespace Kurisu.NGDS
{
    public class DefaultDialogueResolver : IDialogueResolver
    {
        public Dialogue Dialogue { get; private set; }
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(Dialogue dialogue, IDialogueSystem system)
        {
            Dialogue = dialogue;
        }

        public IEnumerator EnterDialogue()
        {
            yield return Dialogue.ProcessModules(ObjectContainer);
        }
        public IEnumerator ExitDialogue()
        {
            Dialogue.Dispose();
            yield break;
        }
    }
}
