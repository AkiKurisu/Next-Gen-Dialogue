using System.Threading;
using System.Threading.Tasks;
namespace Kurisu.NGDS
{
    public interface ITranslator
    {
        Task<string> Translate(string input, CancellationToken ct);
    }
}