using System.Collections.Generic;
using Cysharp.Threading.Tasks;
namespace NextGenDialogue
{
    public interface IOptionResolver
    {
        UniTask EnterOption();
        
        UniTask ClickOption(Option option);
        
        IReadOnlyList<Option> DialogueOptions { get; }
        
        void Inject(IReadOnlyList<Option> options, DialogueSystem system);
    }
}
