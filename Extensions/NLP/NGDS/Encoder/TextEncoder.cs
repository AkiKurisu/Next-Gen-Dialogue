using System;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine.Pool;
namespace Kurisu.NGDS.NLP
{
    public class TextEncoder : IEncoder, IDisposable
    {
        public enum PoolingType
        {
            MeanPooled,
            CLSPooled
        }
        private readonly BertTokenizer tokenizer;
        private readonly IWorker worker;
        public const string last_hidden_state = "last_hidden_state";
        public const string attention_mask = "attention_mask";
        public PoolingType Pooling { get; set; } = PoolingType.MeanPooled;
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
            var pool = ListPool<string>.Get();
            pool.Add(input);
            try
            {
                return Encode(ops, pool);
            }
            finally
            {
                ListPool<string>.Release(pool);
            }
        }
        public TensorFloat Encode(Ops ops, IReadOnlyList<string> input)
        {
            if (Pooling == PoolingType.MeanPooled)
                return Encode_Mean_Pooling(ops, input, true);
            return Encode_CLS_Pooling(ops, input, true);
        }
        public TensorFloat Encode_Mean_Pooling(Ops ops, IReadOnlyList<string> input, bool normalized)
        {
            Dictionary<string, Tensor> inputSentencesTokensTensor = tokenizer.Tokenize(input);
            TensorFloat outputTensor = PeekOutput_Imp(inputSentencesTokensTensor);
            TensorFloat MeanPooledTensor = NLPUtils.MeanPooling(inputSentencesTokensTensor[attention_mask], outputTensor, ops);
            return normalized ? NLPUtils.L2Norm(MeanPooledTensor, ops) : MeanPooledTensor;
        }
        private readonly int[] start = new int[3] { 0, 0, 0 };
        private readonly int[] end = new int[3] { 1, 1, 1 };
        public TensorFloat Encode_CLS_Pooling(Ops ops, IReadOnlyList<string> input, bool normalized)
        {
            Dictionary<string, Tensor> inputSentencesTokensTensor = tokenizer.Tokenize(input);
            TensorFloat outputTensor = PeekOutput_Imp(inputSentencesTokensTensor);
            end[2] = outputTensor.shape[2];
            //Perform pooling. In this case, cls pooling.
            //(batch,sequence_length,embedding_length) => (batch,1,embedding_length)
            TensorFloat CLSPooledTensor = ops.Slice(outputTensor, new(start), new(end), null, null);
            CLSPooledTensor = ops.Reshape(CLSPooledTensor, new TensorShape(outputTensor.shape[0], outputTensor.shape[2]));
            return normalized ? NLPUtils.L2Norm(CLSPooledTensor, ops) : CLSPooledTensor;
        }
        public TensorFloat PeekOutput(IReadOnlyList<string> input)
        {
            Dictionary<string, Tensor> inputSentencesTokensTensor = tokenizer.Tokenize(input);
            worker.Execute(inputSentencesTokensTensor);
            return worker.PeekOutput(last_hidden_state) as TensorFloat;
        }
        private TensorFloat PeekOutput_Imp(Dictionary<string, Tensor> inputSentencesTokensTensor)
        {
            worker.Execute(inputSentencesTokensTensor);
            return worker.PeekOutput(last_hidden_state) as TensorFloat;
        }
    }
}