using UnityEngine;
using Unity.Sentis;
using System;
namespace Kurisu.NGDS.NLP
{
    public class BertEmotion
    {
        private static readonly string[] id2Label = new string[6]{
            "sadness",
            "joy",
            "love",
            "anger",
            "fear",
            "surprise"
        };
        public static string Id2Label(int labelId)
        {
            if (labelId < 0 || labelId > 5) return null;
            return id2Label[labelId];
        }
        public static int Label2Id(string label)
        {
            return Array.IndexOf(id2Label, label);
        }
    }
    public class DistilbertSentiments
    {
        private static readonly string[] id2Label = new string[3]{
            "positive",
            "neutral",
            "negative"
        };
        public static string Id2Label(int labelId)
        {
            if (labelId < 0 || labelId > 2) return null;
            return id2Label[labelId];
        }
        public static int Label2Id(string label)
        {
            return Array.IndexOf(id2Label, label);
        }
    }
    public class SentimentAnalysisEngine : MonoBehaviour
    {
        public TextAsset tokenizerAsset;
        public ModelAsset modelAsset;
        public BackendType backendType = BackendType.CPU;
        private ITensorAllocator allocator;
        private Ops ops;
        private SentimentClassifier classifier;
        /// <summary>
        /// Load the model on awake
        /// </summary>
        private void Awake()
        {
            // Create an allocator.
            allocator = new TensorCachingAllocator();
            // Create an operator
            ops = WorkerFactory.CreateOps(backendType, allocator);
            classifier = new SentimentClassifier(ModelLoader.Load(modelAsset), new BertTokenizer(tokenizerAsset.text), backendType);
        }

        private void OnDisable()
        {
            // Tell the GPU we're finished with the memory the engine used
            classifier.Dispose();
            ops.Dispose();
            allocator.Dispose();
        }
        public int Classify(string inputSentence, out float probability)
        {
            (TensorFloat result, int argMaxIndex) = classifier.Classify(ops, inputSentence);
            result.MakeReadable();
            probability = result[argMaxIndex];
            return argMaxIndex;
        }
    }
}