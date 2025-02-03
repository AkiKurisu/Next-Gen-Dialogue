using System;
using Ceres.Annotations;
using Ceres.Graph;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Kurisu.NGDT
{
    [DisallowMultipleComponent]
    public class NextGenDialogueComponent : MonoBehaviour, IDialogueGraphContainer, IFlowGraphContainer, IFlowGraphRuntime
    {
        [NonSerialized]
        private DialogueGraph _dialogueGraph;
        
        [HideInInspector, SerializeField]
        private DialogueGraphData dialogueGraphData;
        
        [HideInInspector, SerializeField]
        private FlowGraphData flowGraphData;
        
        public UObject Object => gameObject;

        [SerializeField, Tooltip("Create dialogue graph from external asset at runtime")]
        private NextGenDialogueGraphAsset externalAsset;
        
        /// <summary>
        /// Overwrite <see cref="externalAsset"/> to create dialogue graph from external asset,
        /// and leave null to use component embedded data.
        /// </summary>
        /// <value></value>
        public NextGenDialogueGraphAsset Asset 
        { 
            get => externalAsset;
            set
            {
                externalAsset = value;
                if (_dialogueGraph == null) return;
                
                _dialogueGraph.Dispose();
                _dialogueGraph = null;
                (_dialogueGraph = GetDialogueGraph()).Compile();
            } 
        }

        // ============= Implementation for IFlowGraphRuntime =================== //
        FlowGraph IFlowGraphRuntime.Graph => GetDialogueGraphInstance().FlowGraph;
        // ============= Implementation for IFlowGraphRuntime =================== //

        /* Migration from v1 */
#if UNITY_EDITOR
        [SerializeReference, HideInInspector]
        internal Root root = new();
        
        [SerializeReference, HideInInspector]
        internal SharedVariable[] sharedVariables;
#endif
        
        private void Awake()
        {
            (_dialogueGraph = GetDialogueGraph()).Compile();
        }

        private void OnDestroy()
        {
            _dialogueGraph?.Dispose();
        }

        /// <summary>
        /// Get or create dialogue graph instance from this component
        /// </summary>
        /// <returns></returns>
        public DialogueGraph GetDialogueGraphInstance()
        {
            return _dialogueGraph ?? GetDialogueGraph();
        }

        /// <summary>
        /// Play dialogue
        /// </summary>
        [ExecutableFunction, CeresLabel("Play Dialogue")]
        public void PlayDialogue()
        {
            GetDialogueGraphInstance().PlayDialogue(this);
        }

        /// <summary>
        /// Play dialogue from <see cref="NextGenDialogueGraphAsset"/>
        /// </summary>
        /// <param name="asset"></param>
        [ExecutableFunction, CeresLabel("Play Dialogue from Asset")]
        public void PlayDialogue(NextGenDialogueGraphAsset asset)
        {
            Asset = asset;
            PlayDialogue();
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            if (Asset)
            {
                return Asset.GetDialogueGraph();
            }
            
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
