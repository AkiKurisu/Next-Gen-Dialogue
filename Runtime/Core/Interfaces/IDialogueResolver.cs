using System.Threading;
using Cysharp.Threading.Tasks;

namespace NextGenDialogue
{
    public interface IDialogueResolver
    {
        UniTask EnterDialogue(CancellationToken cancellationToken);
        
        UniTask ExitDialogue();
        
        Dialogue Dialogue { get; }
        
        void Inject(Dialogue dialogue, DialogueSystem system);
    }
}
