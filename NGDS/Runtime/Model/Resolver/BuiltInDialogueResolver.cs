using System.Collections;
namespace Kurisu.NGDS
{
    public class BuiltInDialogueResolver : IDialogueResolver
    {
        public Dialogue Dialogue { get; private set; }
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(Dialogue dialogue, IDialogueSystem system)
        {
            Dialogue = dialogue;
        }

        public IEnumerator EnterDialogue()
        {
            for (int i = 0; i < Dialogue.Modules.Count; i++)
            {
                if (Dialogue.Modules[i] is IProcessable injectable)
                    yield return injectable.Process(ObjectContainer);
            }
        }
        public IEnumerator ExitDialogue() { yield break; }
    }
}
