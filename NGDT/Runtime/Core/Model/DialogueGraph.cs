using System;
using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using Kurisu.NGDS;
using UnityEngine;
using UnityEngine.Assertions;
namespace Kurisu.NGDT
{
    [Serializable]
    public class DialogueGraphData : LinkedGraphData
    {
        public DialogueGraphData()
        {
            
        }
        
        public DialogueGraphData(DialogueGraph graph) : base(graph)
        {
        
        }

        protected override CeresNode GetFallbackNode(CeresNodeData fallbackNodeData, int index)
        {
            /* Variants for fallback nodes */
            if (edges[index].children.Length > 0)
            {
                return new InvalidComposite
                {
                    nodeType = fallbackNodeData.nodeType.ToString(),
                    serializedData = fallbackNodeData.serializedData
                };
            }
            
            return new InvalidAction
            {
                nodeType = fallbackNodeData.nodeType.ToString(),
                serializedData = fallbackNodeData.serializedData
            };
        }
        
        public bool IsValid()
        {
            return nodes != null && nodes.Length > 0;
        }
    }

    [Serializable]
    public class DialogueGraph: CeresGraph
    {
        public DialogueGraph()
        {
            
        }

        public DialogueGraph(DialogueGraphData graphData): base(graphData)
        {
        }
        
#if UNITY_EDITOR
        internal DialogueGraph(NextGenDialogueGraphAsset asset)
        {
            LogWarning($"Dialogue graph asset {asset.name} version is outdated, please re-save asset before build.");
            variables = new List<SharedVariable>();
            foreach (var variable in asset.sharedVariables)
            {
                variables.Add(variable.Clone());
            }
            TraverseAppend(asset.root);
            nodeGroups = new List<NodeGroup>();
        }
        
        internal DialogueGraph(NextGenDialogueComponent component)
        {
            LogWarning($"Dialogue graph component {component.name} version is outdated, please re-save component before build.");
            variables = new List<SharedVariable>();
            foreach (var variable in component.sharedVariables)
            {
                variables.Add(variable.Clone());
            }
            TraverseAppend(component.root);
            nodeGroups = new List<NodeGroup>();
        }
#endif

        public Root Root
        {
            get
            {
                if (nodes == null || nodes.Count == 0) return null;
                return (Root)nodes[0];
            }
        }
        
        private DialogueBuilder _builder;

        public IDialogueBuilder Builder 
        {
            get
            {
                return _builder ??= new DialogueBuilder();
            }
        }
        
        /// <summary>
        /// Traverse dialogue graph from root and append node instances
        /// </summary>
        /// <param name="root"></param>
        public void TraverseAppend(Root root)
        {
            nodes = new List<CeresNode> { root };
            nodes.AddRange(root);
        }

        public override void Compile()
        {
            foreach (var variable in variables)
            {
                if (variable is PieceID pieceID)
                {
                    pieceID.Value = Guid.NewGuid().ToString();
                }
            }
            InitVariables_Imp(this);
            BlackBoard.MapGlobal();
        }

        public override void Dispose()
        {
            base.Dispose();
            Root?.Dispose();
        }
        
        /// <summary>
        /// Play the dialogue graph
        /// </summary>
        public void PlayDialogue(GameObject gameObject)
        {
            ((DialogueBuilder)Builder).Clear();
            Root.Abort();
            Root.Run(gameObject, this);
            Root.Awake();
            Root.Start();
            Root.Update();
        }
        
        /// <summary>
        /// Get better format data for serialization of this graph
        /// </summary>
        /// <returns></returns>
        public DialogueGraphData GetData()
        {
#if UNITY_EDITOR
            // Should not serialize data in playing mode which will modify behavior tree structure
            Assert.IsFalse(Application.isPlaying);
#endif
            // since this function used in editor most time
            // use clone to prevent modify source tree
            return new DialogueGraphData(this).CloneT<DialogueGraphData>();
        }
        
        private class DialogueBuilder : IDialogueBuilder
        {
            private readonly Stack<Node> _nodesBuffer = new();
            
            public void StartWriteNode(Node node)
            {
                _nodesBuffer.Push(node);
            }
            
            public void DisposeWriteNode()
            {
                _nodesBuffer.Pop().Dispose();
            }
            
            public Node GetNode()
            {
                return _nodesBuffer.Peek();
            }
            
            public void EndWriteNode()
            {
                var node = _nodesBuffer.Pop();
                if (_nodesBuffer.TryPeek(out var parentNode) && node is IDialogueModule module)
                {
                    parentNode.AddModule(module);
                }
            }
            
            public void Clear()
            {
                _nodesBuffer.Clear();
            }
            
            public void EndBuildDialogue(IDialogueLookup dialogue)
            {
                DialogueSystem.Get().StartDialogue(dialogue);
            }
        }
    }
}
