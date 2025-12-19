using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
namespace NextGenDialogue
{
    public interface IOptionResolver
    {
        UniTask EnterOption(CancellationToken cancellationToken);
        
        UniTask ClickOption(Option option);
        
        IReadOnlyList<Option> DialogueOptions { get; }
        
        void Inject(IReadOnlyList<Option> options, DialogueSystem system);
    }
}
