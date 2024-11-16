using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using UnityEngine;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [DisallowMultipleComponent]
    public class NextGenDialogueComponent : MonoBehaviour, IDialogueContainer
    {
        [HideInInspector, SerializeReference]
        private Root root = new();
        
        UObject ICeresGraphContainer.Object => gameObject;

        [HideInInspector, SerializeReference]
        private List<SharedVariable> sharedVariables = new();
        
        [SerializeField, Tooltip("Create dialogue graph from external dialogue asset at runtime")]
        private NextGenDialogueAsset externalDialogueAsset;
        
        /// <summary>
        /// Overwrite external dialogueTreeAsset to use external data, and leave null to use embedded data.
        /// </summary>
        /// <value></value>
        public NextGenDialogueAsset ExternalData 
        { 
            get => externalDialogueAsset;
            set
            {
                externalDialogueAsset = value;
                if (graphInstance != null)
                {
                    graphInstance.Dispose();
                    graphInstance = null;
                    InitDialogueGraph(graphInstance = GetDialogueGraph());
                }
            } 
        }
        
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

        private DialogueGraph graphInstance = null;

        private void Awake()
        {
            InitDialogueGraph(graphInstance = GetDialogueGraph());
        }

        private void OnDestroy()
        {
            graphInstance?.Dispose();
        }

        public CeresGraph GetGraph()
        {
            return GetDialogueGraph();
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            return graphInstance ?? new DialogueGraph(externalDialogueAsset ? externalDialogueAsset : this);
        }

        protected virtual void InitDialogueGraph(DialogueGraph instance)
        {
            instance.InitVariables();
            instance.BlackBoard.MapGlobal();
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
