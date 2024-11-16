using UnityEngine;
using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "NextGenDialogueTreeAsset", menuName = "Next Gen Dialogue/NextGenDialogueTreeAsset")]
    public class NextGenDialogueTreeAsset : ScriptableObject, IDialogueTree
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
        private List<NodeGroupBlock> blockData = new();
        
        public List<NodeGroupBlock> BlockData => blockData;
        
#if UNITY_EDITOR
        [Multiline, SerializeField]
        private string description;
#endif
        
        [HideInInspector, SerializeReference]
        protected List<SharedVariable> sharedVariables = new();
       
        public void Deserialize(string serializedData)
        {
            if (CeresGraphData.Deserialize(serializedData, typeof(DialogueGraphData)) is not CeresGraphData graphData) return;
            SetGraphData(graphData);
        }
        
        public CeresGraph GetGraph()
        {
            return new DialogueGraph(this);
        }

        public void SetGraphData(CeresGraphData graphData)
        {
            var dialogueGraph = new DialogueGraph(graphData as DialogueGraphData);
            root = dialogueGraph.Root;
            blockData = dialogueGraph.nodeGroupBlocks;
            sharedVariables = dialogueGraph.variables;
        }
    }
}
