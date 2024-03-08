using System;
using System.Collections.Generic;
using Unity.Sentis;
namespace Kurisu.NGDS.NLP
{
    public class SentimentClassifier : IClassifier, IDisposable
    {
        private readonly IWorker worker;
        private readonly BertTokenizer tokenizer;
        private readonly List<string> inputs = new();
        public SentimentClassifier(Model model, BertTokenizer tokenizer, BackendType backendType = BackendType.GPUCompute)
        {
            this.tokenizer = tokenizer;
            worker = WorkerFactory.CreateWorker(backendType, model);
        }
        public void Dispose()
        {
            worker.Dispose();
        }
        public TensorFloat Encode(Ops ops, string input)
        {
            inputs.Clear();
            inputs.Add(input);
            return Encode(ops, inputs);
        }
        public TensorFloat Encode(Ops ops, IReadOnlyList<string> input)
        {
            Dictionary<string, Tensor> inputSentencesTokensTensor = tokenizer.Tokenize(input);
            worker.Execute(inputSentencesTokensTensor);
            TensorFloat outputTensor = ops.Softmax(worker.PeekOutput("logits") as TensorFloat);
            return outputTensor;
        }
        public (TensorFloat, int) Classify(Ops ops, string input)
        {
            var inputTensor = Encode(ops, input);
            TensorInt ids = ops.ArgMax(inputTensor, 1, true);
            ids.MakeReadable();
            int id = ids[0];
            return (inputTensor, id);
        }
        public (TensorFloat, TensorInt) Classify(Ops ops, IReadOnlyList<string> inputs)
        {
            var inputTensor = Encode(ops, inputs);
            TensorInt ids = ops.ArgMax(inputTensor, 1, true);
            return (inputTensor, ids);
        }
    }
}