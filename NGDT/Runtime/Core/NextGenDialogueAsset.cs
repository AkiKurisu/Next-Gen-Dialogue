using UnityEngine;
using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "NextGenDialogueAsset", menuName = "Next Gen Dialogue/NextGenDialogueAsset")]
    public class NextGenDialogueAsset : ScriptableObject, IDialogueContainer
    {
        [SerializeReference, HideInInspector]
        protected Root root = new();
        
        UObject ICeresGraphContainer.Object => this;

        public Root Root
        {
            get => root;
        }
        
        public List<SharedVariable> SharedVariables => sharedVariables;
        
        [SerializeField, HideInInspector]
        private List<NodeGroup> blockData = new();
        
        public List<NodeGroup> BlockData => blockData;
        
        [Multiline, SerializeField]
        private string description;
        
        [HideInInspector, SerializeReference]
        protected List<SharedVariable> sharedVariables = new();
       
        public void Deserialize(string serializedData)
        {
            if (CeresGraphData.Deserialize(serializedData, typeof(DialogueGraphData)) is not CeresGraphData graphData)
            {
                return;
            }
            SetGraphData(graphData);
        }
        
        public CeresGraph GetGraph()
        {
            return GetDialogueGraph();
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            return new DialogueGraph(this);
        }

        public void SetGraphData(CeresGraphData graphData)
        {
            var dialogueGraph = new DialogueGraph(graphData as DialogueGraphData);
            root = dialogueGraph.Root;
            blockData = dialogueGraph.nodeGroups;
            sharedVariables = dialogueGraph.variables;
        }
    }
}
