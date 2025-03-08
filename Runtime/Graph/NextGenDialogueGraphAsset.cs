using UnityEngine;
using Ceres.Graph;
using Ceres.Graph.Flow;
using UObject = UnityEngine.Object;

namespace NextGenDialogue.Graph
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Next Gen Dialogue/Dialogue Graph Asset")]
    public class NextGenDialogueGraphAsset : ScriptableObject, IDialogueGraphContainer, IFlowGraphContainer
    {
        [HideInInspector, SerializeField]
        private DialogueGraphData dialogueGraphData;

        [HideInInspector, SerializeField] 
        private FlowGraphData flowGraphData;
        
        public UObject Object => this;
        
        [Multiline, SerializeField]
        private string description;

        [SerializeField] 
        private FlowGraphAsset flowGraphAsset;
        
        public DialogueGraph GetDialogueGraph()
        {
            dialogueGraphData ??= new DialogueGraphData();
            return new DialogueGraph(dialogueGraphData, this);
        }

        public void SetGraphData(CeresGraphData graphData)
        {
            switch (graphData)
            {
                case DialogueGraphData dialogue:
                    dialogueGraphData = dialogue;
                    break;
                case FlowGraphData flow:
                    flowGraphData = flow;
                    break;
            }
        }

        public FlowGraph GetFlowGraph()
        {
            if (flowGraphAsset)
            {
                return flowGraphAsset.GetFlowGraph();
            }
            flowGraphData ??= new FlowGraphData();
            return flowGraphData.CreateFlowGraphInstance();
        }
        
        FlowGraphData IFlowGraphContainer.GetFlowGraphData()
        {
            return flowGraphData;
        }
    }
}
