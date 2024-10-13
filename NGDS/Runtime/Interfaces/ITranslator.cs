using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface ITranslator
    {
        Task<string> TranslateAsync(string input, CancellationToken ct);
    }
}