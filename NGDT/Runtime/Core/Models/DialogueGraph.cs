using System;
using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using Ceres.Graph.Flow;
using Kurisu.NGDS;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;

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
                return new InvalidCompositeNode
                {
                    nodeType = fallbackNodeData.nodeType.ToString(),
                    serializedData = fallbackNodeData.serializedData
                };
            }
            
            return new InvalidActionNode
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

        public DialogueGraph(DialogueGraphData graphData, IFlowGraphContainer flowGraphContainer): base(graphData)
        {
            FlowGraph = flowGraphContainer.GetFlowGraph();
        }
        
#if UNITY_EDITOR
        internal DialogueGraph(NextGenDialogueGraphAsset asset)
        {
            CeresAPI.LogWarning($"Dialogue graph asset {asset.name} version is outdated, please re-save asset before build.");
            variables = new List<SharedVariable>();
            if (asset.sharedVariables != null)
            {
                foreach (var variable in asset.sharedVariables)
                {
                    if (variable != null)
                    {
                        variables.Add(variable.Clone());
                    }
                }
            }
            TraverseAppend(asset.root);
            nodeGroups = new List<NodeGroup>();
            FlowGraph = asset.GetFlowGraph();
        }
        
        internal DialogueGraph(NextGenDialogueComponent component)
        {
            CeresAPI.LogWarning($"Dialogue graph component {component.name} version is outdated, please re-save component before build.");
            variables = new List<SharedVariable>();
            if (component.sharedVariables != null)
            {
                foreach (var variable in component.sharedVariables)
                {
                    if (variable != null)
                    {
                        variables.Add(variable.Clone());
                    }
                }
            }

            TraverseAppend(component.root);
            nodeGroups = new List<NodeGroup>();
            FlowGraph = component.GetFlowGraph();
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

        public DialogueBuilder Builder 
        {
            get
            {
                return _builder ??= new DialogueBuilder();
            }
        }
        
        public FlowGraph FlowGraph { get; private set; }
        
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
            FlowGraph?.Compile();
        }

        public override void Dispose()
        {
            base.Dispose();
            Root?.Dispose();
            FlowGraph?.Dispose();
        }
        
        /// <summary>
        /// Play the dialogue graph
        /// </summary>
        public void PlayDialogue(NextGenDialogueComponent component)
        {
            ((DialogueBuilder)Builder).Clear();
            Root.Abort();
            foreach (var node in nodes)
            {
                ((DialogueNode)node).Initialize(component, this);
            }
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
    }
    
    public class DialogueBuilder
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

        public TNode GetFirstAncestorOfType<TNode>() where TNode : Node
        {
            TNode result = null;
            using (ListPool<Node>.Get(out var list))
            {
                while (_nodesBuffer.TryPop(out var node))
                {
                    list.Add(node);
                    if (node is TNode tNode)
                    {
                        result = tNode;
                        break;
                    }
                }

                for (int i = list.Count - 1; i >= 0; i--)
                {
                    _nodesBuffer.Push(list[i]);
                }
            }
            
            return result;
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
