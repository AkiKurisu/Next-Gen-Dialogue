using UnityEngine;
using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "NextGenDialogueAsset", menuName = "Next Gen Dialogue/NextGenDialogueAsset")]
    public class NextGenDialogueGraphAsset : ScriptableObject, IDialogueGraphContainer
    {
        [SerializeReference, HideInInspector]
        protected Root root = new();
        
        public UObject Object => this;

        public Root Root => root;

        public List<SharedVariable> SharedVariables => sharedVariables;
        
        [SerializeField, HideInInspector]
        private List<NodeGroup> nodeGroups = new();
        
        public List<NodeGroup> NodeGroups => nodeGroups;
        
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
            var dialogueGraph = new DialogueGraph((DialogueGraphData)graphData);
            root = dialogueGraph.Root;
            nodeGroups = dialogueGraph.nodeGroups;
            sharedVariables = dialogueGraph.variables;
        }
    }
}
