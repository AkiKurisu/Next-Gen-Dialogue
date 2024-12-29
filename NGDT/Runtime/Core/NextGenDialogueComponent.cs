using System;
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
                if (_graphInstance == null) return;
                
                _graphInstance.Dispose();
                _graphInstance = null;
                (_graphInstance = GetDialogueGraph()).Compile();
            } 
        }
        
        [SerializeField, HideInInspector]
        private List<NodeGroup> nodeGroups = new();
        
        public List<NodeGroup> NodeGroups => nodeGroups;
        
        public Root Root
        {
            get => root;
        }
        
        public List<SharedVariable> SharedVariables
        {
            get => sharedVariables;
        }

        [NonSerialized]
        private DialogueGraph _graphInstance;

        private void Awake()
        {
            (_graphInstance = GetDialogueGraph()).Compile();
        }

        private void OnDestroy()
        {
            _graphInstance?.Dispose();
        }

        public CeresGraph GetGraph()
        {
            return GetDialogueGraph();
        }
        
        public DialogueGraph GetDialogueGraph()
        {
            return _graphInstance ?? new DialogueGraph(externalDialogueAsset ? externalDialogueAsset : this);
        }

        public void SetGraphData(CeresGraphData graphData)
        {
            var dialogueGraph = new DialogueGraph(graphData as DialogueGraphData);
            root = dialogueGraph.Root;
            nodeGroups = dialogueGraph.nodeGroups;
            sharedVariables = dialogueGraph.variables;
        }
    }
}
