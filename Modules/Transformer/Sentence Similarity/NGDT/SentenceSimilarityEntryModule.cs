using System.Collections.Generic;
using Kurisu.NGDS.Transformer.SS;
using UnityEngine;
namespace Kurisu.NGDT.Transformer.SS
{
    [AkiInfo("Module : Sentence Similarity Entry Module is the entry point to collect sentence data.")]
    [ModuleOf(typeof(Dialogue))]
    [AkiGroup("AIGC/Transformer")]
    public class SentenceSimilarityEntryModule : Module
    {
        [SerializeField, Tooltip("Sentence similarity inference engine")]
        private SharedTObject<SentenceSimilarityEngine> ssEngine;
        [SerializeField, ForceShared]
        private SharedTObject<SentenceSimilarityDataSet> ssDataSet;
        [SerializeField]
        private SharedFloat minScore;
        public override void Awake()
        {
            InitVariable(ssDataSet);
            InitVariable(ssEngine);
            InitVariable(minScore);
        }
        protected sealed override Status OnUpdate()
        {
            var entry = ScriptableObject.CreateInstance<SentenceSimilarityDataSet>();
            entry.minScore = minScore.Value;
            entry.engine = ssEngine.Value;
            var sentences = new List<string>();
            var ids = new List<string>();
            //Search similar piece
            var map = Tree.Root.GetActiveDialogue().ToReadOnlyPieceMap();
            foreach (var pair in map)
            {
                foreach (var node in pair.Value.Traverse(false))
                {
                    if (node is IExposedContent exposedContent)
                    {
                        sentences.Add(exposedContent.GetContent());
                        ids.Add(pair.Key);
                        continue;
                    }
                }
            }
            entry.comparisonSentences = sentences.ToArray();
            entry.pieceIDs = ids.ToArray();
            ssDataSet.Value = entry;
            return Status.Success;
        }
    }
}
