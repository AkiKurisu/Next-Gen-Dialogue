using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface IDialogueResolver
    {
        Task OnDialogueEnter();
        void OnDialogueExit();
        Dialogue Dialogue { get; }
        void Inject(Dialogue dialogue, IDialogueSystem system);
    }
}
