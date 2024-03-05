using UnityEngine;
namespace Kurisu.NGDT.NLP.SS
{
    [AkiInfo("Module : Inference Similar ID Module is used to inference option's target dialogue piece id using Sentence Similarity " +
    "powered by transformer model inference at runtime.")]
    [ModuleOf(typeof(Option))]
    [AkiGroup("NLP")]
    public class InferenceSimilarIDModule : Module
    {
        [Tooltip("Sentence similarity inference dataSet"), ForceShared]
        public SharedTObject<SentenceSimilarityDataSet> ssDataSet;
        protected sealed override Status OnUpdate()
        {
            var parentNode = Tree.Builder.GetNode() as NGDS.Option;
            if (ssDataSet.Value.TryGetSimilarID(parentNode.Content, out string targetID))
                parentNode.AddModule(new NGDS.TargetIDModule(targetID));
            return Status.Success;
        }
    }
}
