using System.Collections;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public interface IOptionResolver
    {
        IEnumerator EnterOption();
        IEnumerator ClickOption(Option option);
        IReadOnlyList<Option> DialogueOptions { get; }
        void Inject(IReadOnlyList<Option> options, IDialogueSystem system);
    }
}
