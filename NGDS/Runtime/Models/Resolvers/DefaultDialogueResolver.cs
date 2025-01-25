using Cysharp.Threading.Tasks;
namespace Kurisu.NGDS
{
    public class DefaultDialogueResolver : IDialogueResolver
    {
        public Dialogue Dialogue { get; private set; }
        
        protected ObjectContainer ObjectContainer { get; } = new();
        
        public void Inject(Dialogue dialogue, DialogueSystem system)
        {
            Dialogue = dialogue;
        }

        public UniTask EnterDialogue()
        {
            return Dialogue.ProcessModules(ObjectContainer);
        }
        
        public UniTask ExitDialogue()
        {
            Dialogue.Dispose();
            return UniTask.CompletedTask;
        }
    }
}
