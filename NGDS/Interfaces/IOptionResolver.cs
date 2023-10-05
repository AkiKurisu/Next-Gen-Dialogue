using System.Collections;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public interface IOptionResolver
    {
        IEnumerator EnterOption();
        IEnumerator ClickOption(DialogueOption option);
        IReadOnlyList<DialogueOption> DialogueOptions { get; }
        void Inject(IReadOnlyList<DialogueOption> options, IDialogueSystem system);
    }
}
