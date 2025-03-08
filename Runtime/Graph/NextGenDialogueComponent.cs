using System;
using Ceres.Annotations;
using Ceres.Graph;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace NextGenDialogue.Graph
{
    [DisallowMultipleComponent]
    public class NextGenDialogueComponent : MonoBehaviour, 
        IDialogueGraphContainer,
        IFlowGraphContainer, IFlowGraphRuntime
    {
        [NonSerialized]
        private DialogueGraph _dialogueGraph;
        
        [HideInInspector, SerializeField]
        private DialogueGraphData dialogueGraphData;
        
        [HideInInspector, SerializeField]
        private FlowGraphData flowGraphData;
        
        public UObject Object => this;

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
                /* Clear cache before get instance */
                _dialogueGraph = null;
                _dialogueGraph = GetDialogueGraph();
                using var context = FlowGraphCompilationContext.GetPooled();
                using var compiler = CeresGraphCompiler.GetPooled(_dialogueGraph, context);
                _dialogueGraph.Compile(compiler);
            } 
        }

        /// <summary>
        /// Get runtime dialogue graph instance
        /// </summary>
        public DialogueGraph DialogueGraph
        {
            get
            {
                if (_dialogueGraph == null)
                {
                    _dialogueGraph = GetDialogueGraph();
                    using var context = FlowGraphCompilationContext.GetPooled();
                    using var compiler = CeresGraphCompiler.GetPooled(_dialogueGraph, context);
                    _dialogueGraph.Compile(compiler);
                }

                return _dialogueGraph;
            }
        }
        

        // ============= Implementation for IFlowGraphRuntime =================== //
        FlowGraph IFlowGraphRuntime.Graph => DialogueGraph.FlowGraph;
        // ============= Implementation for IFlowGraphRuntime =================== //

        private void Awake()
        {
            _dialogueGraph = GetDialogueGraph();
            using var context = FlowGraphCompilationContext.GetPooled();
            using var compiler = CeresGraphCompiler.GetPooled(_dialogueGraph, context);
            _dialogueGraph.Compile(compiler);
        }

        private void OnDestroy()
        {
            _dialogueGraph?.Dispose();
        }

        /// <summary>
        /// Play dialogue
        /// </summary>
        [ExecutableFunction, CeresLabel("Play Dialogue")]
        public void PlayDialogue()
        {
            DialogueGraph.PlayDialogue(this);
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
#if UNITY_EDITOR
            if (Application.isPlaying && _dialogueGraph != null)
            {
                return _dialogueGraph;
            }
#endif
            
            if (Asset)
            {
                return Asset.GetDialogueGraph();
            }
            
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
        
        FlowGraphData IFlowGraphContainer.GetFlowGraphData()
        {
            return flowGraphData;
        }

        public FlowGraph GetFlowGraph()
        {
            flowGraphData ??= new FlowGraphData();
            return flowGraphData.CreateFlowGraphInstance();
        }
    }
}
