using System.Threading;
using Cysharp.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface ITranslator
    {
        UniTask<string> TranslateAsync(string input, CancellationToken ct);
    }
}