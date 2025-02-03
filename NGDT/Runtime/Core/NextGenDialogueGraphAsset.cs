using UnityEngine;
using Ceres.Graph;
using Ceres.Graph.Flow;
using UnityEngine.Serialization;
using UObject = UnityEngine.Object;

namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Next Gen Dialogue/Dialogue Graph Asset")]
    public class NextGenDialogueGraphAsset : ScriptableObject, IDialogueGraphContainer, IFlowGraphContainer
    {
        [FormerlySerializedAs("graphData")] 
        [HideInInspector, SerializeField]
        private DialogueGraphData dialogueGraphData;

        [HideInInspector, SerializeField] 
        private FlowGraphData flowGraphData;
        
        public UObject Object => this;
        
        [Multiline, SerializeField]
        private string description;

        [SerializeField] 
        private FlowGraphAsset flowGraphAsset;
        
        /* Migration from v1 */
#if UNITY_EDITOR
        [SerializeReference, HideInInspector]
        internal Root root = new();
        
        [SerializeReference, HideInInspector]
        internal SharedVariable[] sharedVariables;
#endif
        
        public void Deserialize(string serializedData)
        {
            var data = CeresGraphData.FromJson<DialogueGraphData>(serializedData);
            if (data == null)
            {
                return;
            }
            SetGraphData(data);
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            dialogueGraphData ??= new DialogueGraphData();
#if UNITY_EDITOR
            if (!dialogueGraphData.IsValid())
            {
                return new DialogueGraph(this);
            }
#endif
            return new DialogueGraph(dialogueGraphData.CloneT<DialogueGraphData>(), this);
        }

        public void SetGraphData(CeresGraphData graph)
        {
            if (graph is DialogueGraphData dialogue)
            {
                dialogueGraphData = dialogue;
#if UNITY_EDITOR
                root = null;
                sharedVariables = null;
#endif
            }
            else if (graph is FlowGraphData flow)
            {
                flowGraphData = flow;
            }
        }

        public FlowGraph GetFlowGraph()
        {
            if (flowGraphAsset)
            {
                return flowGraphAsset.GetFlowGraph();
            }
            flowGraphData ??= new FlowGraphData();
            return new FlowGraph(flowGraphData.CloneT<FlowGraphData>());
        }
    }
}
