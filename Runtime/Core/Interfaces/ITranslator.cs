using System.Threading;
using Cysharp.Threading.Tasks;
namespace NextGenDialogue
{
    public interface ITranslator
    {
        UniTask<string> TranslateAsync(string input, CancellationToken ct);
    }
}