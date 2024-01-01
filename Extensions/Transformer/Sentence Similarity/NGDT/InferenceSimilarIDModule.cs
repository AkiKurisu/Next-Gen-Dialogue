using UnityEngine;
namespace Kurisu.NGDT.Transformer.SS
{
    [AkiInfo("Module : Inference Similar ID Module is used to inference option's target dialogue piece id using Sentence Similarity " +
    "powered by transformer model inference at runtime.")]
    [ModuleOf(typeof(Option))]
    [AkiGroup("AIGC/Transformer")]
    public class InferenceSimilarIDModule : Module
    {
        [SerializeField, Tooltip("Sentence similarity inference dataSet"), ForceShared]
        private SharedTObject<SentenceSimilarityDataSet> ssDataSet;
        public override void Awake()
        {
            InitVariable(ssDataSet);
        }
        protected sealed override Status OnUpdate()
        {
            var parentNode = Tree.Builder.GetNode() as NGDS.Option;
            if (ssDataSet.Value.TryGetSimilarID(parentNode.Content, out string targetID))
                parentNode.AddModule(new NGDS.TargetIDModule(targetID));
            return Status.Success;
        }
    }
}
