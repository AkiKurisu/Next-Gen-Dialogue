using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using UnityEngine;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [DisallowMultipleComponent]
    public class NextGenDialogueTree : MonoBehaviour, IDialogueTree
    {
        [HideInInspector, SerializeReference]
        private Root root = new();
        
        UObject ICeresGraphContainer.Object => gameObject;

        [HideInInspector, SerializeReference]
        private List<SharedVariable> sharedVariables = new();
        
        [SerializeField, Tooltip("Replace the dialogue tree in the component with the external dialogue tree," +
        " which will overwrite the dialogue tree in the component when saving")]
        private NextGenDialogueTreeAsset externalDialogueTree;
        
        /// <summary>
        /// Overwrite external dialogueTreeAsset to use external data, and leave null to use embedded data.
        /// </summary>
        /// <value></value>
        public NextGenDialogueTreeAsset ExternalData { get => externalDialogueTree; set => externalDialogueTree = value; }
        
        [SerializeField, HideInInspector]
        private List<NodeGroupBlock> blockData = new();
        
        public List<NodeGroupBlock> BlockData => blockData;
        
        public Root Root
        {
            get => root;
        }
        
        public List<SharedVariable> SharedVariables
        {
            get => sharedVariables;
        }

        private DialogueGraph graphInstance;

        private void Awake()
        {
            var instance = GetDialogueGraph();
            // Init instance
            instance.InitVariables();
            instance.BlackBoard.MapGlobal();
        }

        public CeresGraph GetGraph()
        {
            return GetDialogueGraph();
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            return graphInstance ??= new DialogueGraph(externalDialogueTree ? externalDialogueTree : this);
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
