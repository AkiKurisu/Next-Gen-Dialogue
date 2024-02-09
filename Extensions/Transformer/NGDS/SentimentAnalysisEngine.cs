using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
namespace Kurisu.NGDS.Transformer
{
    public class SentimentAnalysisEngine : MonoBehaviour
    {
        public TextAsset tokenizer;
        public ModelAsset modelAsset;
        public BackendType backendType = BackendType.CPU;
        public Model runtimeModel;
        public IWorker worker;
        public ITensorAllocator allocator;
        public Ops ops;
        private JObject tokenizerJsonData;
        private static readonly string[] id2Label = new string[6]{
            "sadness",
            "joy",
            "love",
            "anger",
            "fear",
            "surprise"
        };

        /// <summary>
        /// Load the model on awake
        /// </summary>
        private void Awake()
        {
            // Load the ONNX model
            runtimeModel = ModelLoader.Load(modelAsset);

            // Load tokenizer
            tokenizerJsonData = JsonConvert.DeserializeObject<JObject>(tokenizer.text);

            // Create an engine and set the backend as GPU //GPUCompute
            worker = WorkerFactory.CreateWorker(backendType, runtimeModel);

            // Create an allocator.
            allocator = new TensorCachingAllocator();

            // Create an operator
            ops = WorkerFactory.CreateOps(BackendType.GPUCompute, allocator);
        }

        private void OnDisable()
        {
            // Tell the GPU we're finished with the memory the engine used
            worker.Dispose();
        }
        public static string Id2Label(int labelId)
        {
            if (labelId < 0 || labelId > 6) return null;
            return id2Label[labelId];
        }
        public float[] Classify(string inputSentence)
        {
            List<string> InputSentences = new()
            {
                inputSentence
            };
            // Step 1: Tokenize the sentences
            Dictionary<string, Tensor> inputSentencesTokensTensor = TransformerUtils.TokenizeInput(tokenizerJsonData, InputSentences);

            // Step 2: Compute embedding and get the output
            worker.Execute(inputSentencesTokensTensor);

            // Step 3: Get the output from the neural network
            TensorFloat outputTensor = ops.Softmax(worker.PeekOutput("logits") as TensorFloat);
            outputTensor.MakeReadable();
            // Return probabilities 
            return outputTensor.ToReadOnlyArray();
        }
        public int Classify(string inputSentence, out float probability)
        {
            List<string> InputSentences = new()
            {
                inputSentence
            };
            // Step 1: Tokenize the sentences
            Dictionary<string, Tensor> inputSentencesTokensTensor = TransformerUtils.TokenizeInput(tokenizerJsonData, InputSentences);

            // Step 2: Compute embedding and get the output
            worker.Execute(inputSentencesTokensTensor);
            var output = worker.PeekOutput("logits") as TensorFloat;

            TensorInt ids = ops.ArgMax(output, 1, true);
            ids.MakeReadable();

            int id = ids[0];
            output.MakeReadable();
            probability = output[id];
            return id;
        }
    }
}