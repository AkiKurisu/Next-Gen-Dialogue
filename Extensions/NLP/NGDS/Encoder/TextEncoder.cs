using System;
using System.Collections.Generic;
using Unity.Sentis;
namespace Kurisu.NGDS.NLP
{
    public class TextEncoder : IEncoder, IDisposable
    {
        private readonly BertTokenizer tokenizer;
        private readonly IWorker worker;
        private readonly List<string> inputs = new();
        public TextEncoder(Model model, BertTokenizer tokenizer, BackendType backendType = BackendType.GPUCompute)
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
            TensorFloat outputTensor = worker.PeekOutput("last_hidden_state") as TensorFloat;
            TensorFloat MeanPooledTensor = NLPUtils.MeanPooling(inputSentencesTokensTensor["attention_mask"], outputTensor, ops);
            TensorFloat NormedTensor = NLPUtils.L2Norm(MeanPooledTensor, ops);
            return NormedTensor;
        }
    }
}