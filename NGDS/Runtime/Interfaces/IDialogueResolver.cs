using Cysharp.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface IDialogueResolver
    {
        UniTask EnterDialogue();
        
        UniTask ExitDialogue();
        
        Dialogue Dialogue { get; }
        
        void Inject(Dialogue dialogue, DialogueSystem system);
    }
}
