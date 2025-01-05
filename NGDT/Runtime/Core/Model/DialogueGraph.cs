using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    public class DialogueGraphData : LinkedGraphData
    {
        private DialogueGraphData(DialogueGraph graph) : base(graph)
        {
        
        }

        protected override CeresNode GetFallbackNode(CeresNodeData fallbackNodeData, int index)
        {
            /* Variants for fallback nodes */
            if (edges[index].children.Length > 0)
            {
                return new InvalidComposite()
                {
                    nodeType = fallbackNodeData.nodeType.ToString(),
                    serializedData = fallbackNodeData.serializedData
                };
            }
            
            return new InvalidAction()
            {
                nodeType = fallbackNodeData.nodeType.ToString(),
                serializedData = fallbackNodeData.serializedData
            };
        }

        /// <summary>
        /// Serialize dialogue tree
        /// </summary>
        /// <param name="dialogueGraphTree"></param>
        /// <param name="indented"></param>
        /// <returns></returns>
        public static string Serialize(IDialogueGraphContainer dialogueGraphTree, bool indented = false)
        {
            var graphData = new DialogueGraphData(new DialogueGraph(dialogueGraphTree)).CloneT<DialogueGraphData>();
            var jsonData = Serialize(graphData, indented);
#if UNITY_EDITOR
            /* Patch for editor, should save dialogue graph data instead of saving nodes and variables directly */
            if(!Application.isPlaying)
            {
                dialogueGraphTree.SetGraphData(graphData);
            }
#endif
            return jsonData;
        }
    }

    public class DialogueGraph: CeresGraph
    {
        public Root Root => nodes[0] as Root;
        
        public DialogueGraph(IDialogueGraphContainer dt)
        {
            variables = new List<SharedVariable>();
            foreach (var variable in dt.SharedVariables)
            {
                variables.Add(variable.Clone());
            }

            nodes = new List<CeresNode> { dt.Root };
            nodes.AddRange(dt.Root); /* Traverse dialogue tree */
            nodeGroups = new List<NodeGroup>(dt.NodeGroups);
        }

        public DialogueGraph(DialogueGraphData graphData): base(graphData)
        {
        }

        private DialogueBuilder _builder;

        public IDialogueBuilder Builder {
            get
            {
                return _builder ??= new DialogueBuilder(this);
            }
        }

        public override void Compile()
        {
            foreach (var variable in variables)
            {
                if (variable is PieceID pieceID) pieceID.Value = System.Guid.NewGuid().ToString();
            }
            InitVariables_Imp(this);
            BlackBoard.MapGlobal();
        }

        public override void Dispose()
        {
            base.Dispose();
            Root.Dispose();
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
        
        private class DialogueBuilder : IDialogueBuilder
        {
            public DialogueBuilder(DialogueGraph graph)
            {
                _graph = graph;
            }
            
            private readonly DialogueGraph _graph;
            
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
                if (_nodesBuffer.TryPeek(out Node parentNode) && node is IDialogueModule module)
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
