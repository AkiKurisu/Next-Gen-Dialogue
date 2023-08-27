using System.Collections.Generic;
using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface IOptionResolver
    {
        Task OnOptionEnter();
        Task OnOptionClick(DialogueOption option);
        IReadOnlyList<DialogueOption> DialogueOptions { get; }
        void Inject(IReadOnlyList<DialogueOption> options, IDialogueSystem system);
    }
}
