using Kurisu.NGDS.NLP;
using UnityEngine;
namespace Kurisu.NGDT.NLP.SS
{
    /// <summary>
    /// Runtime inference data wrapper
    /// </summary>
    public class SentenceSimilarityDataSet : ScriptableObject
    {
        public string[] comparisonSentences;
        public string[] pieceIDs;
        public float minScore = 0.2f;
        public SentenceSimilarityEngine engine;
        public bool TryGetSimilarID(string content, out string targetID)
        {
            var turple = engine.RankSimilarityScores(content, comparisonSentences);
            if (turple.Item2 < minScore)
            {
                targetID = null;
                return false;
            }
            targetID = pieceIDs[turple.Item1];
            return true;
        }
    }
}