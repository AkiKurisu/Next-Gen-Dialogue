using System.Collections.Generic;
using Unity.Sentis;
namespace Kurisu.NGDS.NLP
{
    /// <summary>
    /// Classifier returns label from input, and can act as encoder
    /// </summary>
    public interface IClassifier : IEncoder
    {
        (TensorFloat, int) Classify(Ops ops, string input);
        (TensorFloat, TensorInt) Classify(Ops ops, IReadOnlyList<string> inputs);
    }
}