using System.Threading;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue
{
    public class DefaultDialogueResolver : IDialogueResolver
    {
        public Dialogue Dialogue { get; private set; }
        
        protected ObjectContainer ObjectContainer { get; } = new();
        
        public void Inject(Dialogue dialogue, DialogueSystem system)
        {
            Dialogue = dialogue;
        }

        public UniTask EnterDialogue(CancellationToken cancellationToken)
        {
            return Dialogue.ProcessModules(ObjectContainer, cancellationToken);
        }
        
        public UniTask ExitDialogue()
        {
            Dialogue.Dispose();
            return UniTask.CompletedTask;
        }
    }
}
