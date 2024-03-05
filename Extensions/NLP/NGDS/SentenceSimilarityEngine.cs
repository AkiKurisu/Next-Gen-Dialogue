using System;
using System.Linq;
using UnityEngine;
using Unity.Sentis;
namespace Kurisu.NGDS.NLP
{
    //Code from https://thomassimonini.substack.com/p/create-an-ai-robot-npc-using-hugging?r=dq5fg&utm_campaign=post&utm_medium=web
    public class SentenceSimilarityEngine : MonoBehaviour
    {
        public TextAsset tokenizerAsset;
        public ModelAsset modelAsset;
        public BackendType backendType = BackendType.CPU;
        private ITensorAllocator allocator;
        private Ops ops;
        private TextEncoder textEncoder;
        /// <summary>
        /// Load the model on awake
        /// </summary>
        private void Awake()
        {
            textEncoder = new TextEncoder(ModelLoader.Load(modelAsset), new BertTokenizer(tokenizerAsset.text), backendType);

            // Create an allocator.
            allocator = new TensorCachingAllocator();

            // Create an operator
            ops = WorkerFactory.CreateOps(backendType, allocator);
        }


        private void OnDisable()
        {
            // Tell the GPU we're finished with the memory the engine used
            textEncoder.Dispose();
            allocator.Dispose();
            ops.Dispose();
        }

        /// <summary>
        /// We calculate the similarity scores between the input sequence (what the user typed) and the comparison
        /// sequences (the robot action list)
        /// This similarity score is simply the cosine similarity. It is calculated as the cosine of the angle between two vectors. 
        /// It is particularly useful when your texts are not the same length
        /// </summary>
        /// <param name="InputSequence"></param>
        /// <param name="ComparisonSequences"></param>
        /// <returns></returns>
        public TensorFloat SentenceSimilarityScores(TensorFloat InputSequence, TensorFloat ComparisonSequences)
        {
            TensorFloat SentenceSimilarityScores_ = ops.MatMul2D(InputSequence, ComparisonSequences, false, true);
            return SentenceSimilarityScores_;
        }

        /// <summary>
        /// Get the most similar action and its index given the player input
        /// </summary>
        /// <param name="inputSentence"></param>
        /// <param name="comparisonSentences"></param>
        /// <returns></returns>
        public Tuple<int, float> RankSimilarityScores(string inputSentence, string[] comparisonSentences)
        {
            // Encode the input sentences and comparison sentences
            TensorFloat NormEmbedSentences = textEncoder.Encode(ops, inputSentence);
            TensorFloat NormEmbedComparisonSentences = textEncoder.Encode(ops, comparisonSentences.ToList());

            // Calculate the similarity score of the player input with each action
            TensorFloat scores = SentenceSimilarityScores(NormEmbedSentences, NormEmbedComparisonSentences);
            scores.MakeReadable(); // Be able to read this tensor

            // Helper to return only best score and index
            TensorInt scoreIndex = ops.ArgMax(scores, 1, true);
            scoreIndex.MakeReadable();

            int scoreIndexInt = scoreIndex[0];
            scores.MakeReadable();
            float score = scores[scoreIndexInt];

            // Return the similarity score and the action index
            return Tuple.Create(scoreIndexInt, score);
        }
    }
}