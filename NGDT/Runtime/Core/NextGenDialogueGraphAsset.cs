using System.Collections.Generic;
using Ceres;
using UnityEngine;
using Ceres.Graph;
using Ceres.Graph.Flow;
using UnityEngine.Serialization;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Next Gen Dialogue/Dialogue Asset")]
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

        /* Migration from v1 */
#if UNITY_EDITOR
        [SerializeReference, HideInInspector]
        internal Root root = new();
        
        [SerializeReference, HideInInspector]
        internal List<SharedVariable> sharedVariables = new();
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
            if(graph is DialogueGraphData dialogue)
                dialogueGraphData = dialogue;
            if (graph is FlowGraphData flow)
                flowGraphData = flow;
        }

        public FlowGraph GetFlowGraph()
        {
            flowGraphData ??= new FlowGraphData();
            return new FlowGraph(flowGraphData.CloneT<FlowGraphData>());
        }
    }
}
