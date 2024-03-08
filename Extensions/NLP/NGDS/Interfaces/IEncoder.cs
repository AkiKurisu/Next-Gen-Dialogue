using System.Collections.Generic;
using Unity.Sentis;
namespace Kurisu.NGDS.NLP
{
    /// <summary>
    /// Encoder returns embedding vector from inputs
    /// </summary>
    public interface IEncoder
    {
        TensorFloat Encode(Ops ops, string input);
        TensorFloat Encode(Ops ops, IReadOnlyList<string> input);
    }
}