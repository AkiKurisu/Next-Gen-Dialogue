using System;
using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using UnityEngine;
using UObject = UnityEngine.Object;
namespace Kurisu.NGDT
{
    [DisallowMultipleComponent]
    public class NextGenDialogueComponent : MonoBehaviour, IDialogueGraphContainer
    {
        [NonSerialized]
        private DialogueGraph _dialogueGraph;
        
        [HideInInspector, SerializeField]
        private DialogueGraphData graphData;
        
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
        
        /* Migration from v1 */
#if UNITY_EDITOR
        [SerializeReference, HideInInspector]
        internal Root root = new();
        
        [SerializeReference, HideInInspector]
        internal List<SharedVariable> sharedVariables = new();
#endif

        private void Awake()
        {
            (_dialogueGraph = GetDialogueGraph()).Compile();
        }

        private void OnDestroy()
        {
            _dialogueGraph?.Dispose();
        }

        public CeresGraph GetGraph()
        {
            return GetDialogueGraph();
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
        public void PlayDialogue()
        {
            GetDialogueGraphInstance().PlayDialogue(gameObject);
        }

        /// <summary>
        /// Play dialogue from <see cref="NextGenDialogueGraphAsset"/>
        /// </summary>
        /// <param name="asset"></param>
        public void PlayDialogue(NextGenDialogueGraphAsset asset)
        {
            Asset = asset;
            PlayDialogue();
        }

        /// <summary>
        /// Create new dialogue graph from this component
        /// </summary>
        /// <returns></returns>
        public DialogueGraph GetDialogueGraph()
        {
            if (Asset)
            {
                return Asset.GetDialogueGraph();
            }
            
            graphData ??= new DialogueGraphData();
#if UNITY_EDITOR
            if (!graphData.IsValid())
            {
                return new DialogueGraph(this);
            }
#endif
            return new DialogueGraph(graphData.CloneT<DialogueGraphData>());
        }

        public void SetGraphData(CeresGraphData graph)
        {
            graphData = (DialogueGraphData)graph;
        }
    }
}
