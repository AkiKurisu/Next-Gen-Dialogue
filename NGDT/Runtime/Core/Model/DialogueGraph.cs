using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    public class DialogueGraphData : CeresGraphData
    {
        private DialogueGraphData(DialogueGraph graph) : base(graph)
        {
        }

        protected override CeresNode GetFallbackNode(CeresNodeData fallbackNodeData, Edge output)
        {
            /* Variants for fallback nodes */
            if (output.children.Length > 0)
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
        /// <param name="dialogueTree"></param>
        /// <param name="indented"></param>
        /// <param name="serializeEditorData"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string Serialize(IDialogueTree dialogueTree, bool indented = false, bool serializeEditorData = false,
            bool verbose = true)
        {
            var graphData = new DialogueGraphData(new DialogueGraph(dialogueTree)).CloneT<DialogueGraphData>();
            var jsonData = Serialize(graphData, indented, serializeEditorData, verbose);
#if UNITY_EDITOR
            /* Patch for editor, should save dialogue graph data instead of saving nodes and variables directly */
            if(!Application.isPlaying)
            {
                dialogueTree.SetGraphData(graphData);
            }
#endif
            return jsonData;
        }
    }

    public class DialogueGraph: CeresGraph
    {
        public Root Root => nodes[0] as Root;
        
        public DialogueGraph(IDialogueTree dt)
        {
            variables = new List<SharedVariable>();
            foreach (var variable in dt.SharedVariables)
            {
                variables.Add(variable.Clone());
            }

            nodes = new List<CeresNode> { dt.Root };
            nodes.AddRange(dt.Root); /* Traverse dialogue tree */
            nodeGroupBlocks = new List<NodeGroupBlock>(dt.BlockData);
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

        private IDialogueSystem _dialogueSystem;
        
        private void ResolveDialogue(IDialogueLookup dialogue)
        {
            _dialogueSystem ??= IOCContainer.Resolve<IDialogueSystem>();
            if (_dialogueSystem != null)
            {
                _dialogueSystem.StartDialogue(dialogue);
            }
            else
            {
                Debug.LogError("[NGDT] No dialogue system registered!");
            }
        }

        public override void InitVariables()
        {
            foreach (var variable in variables)
            {
                if (variable is PieceID pieceID) pieceID.Value = System.Guid.NewGuid().ToString();
            }
            base.InitVariables();
        }
        
        public void Run(GameObject gameObject)
        {
            Root.Run(gameObject, this);
            Root.Awake();
            Root.Start();
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
            Run(gameObject);
            Root.Update();
        }
        
        private class DialogueBuilder : IDialogueBuilder
        {
            public DialogueBuilder(DialogueGraph graph)
            {
                this.graph = graph;
            }
            
            private readonly DialogueGraph graph;
            
            private readonly Stack<Node> nodesBuffer = new();
            
            public void StartWriteNode(Node node)
            {
                nodesBuffer.Push(node);
            }
            
            public void DisposeWriteNode()
            {
                nodesBuffer.Pop().Dispose();
            }
            
            public Node GetNode()
            {
                return nodesBuffer.Peek();
            }
            
            public void EndWriteNode()
            {
                var node = nodesBuffer.Pop();
                if (nodesBuffer.TryPeek(out Node parentNode) && node is IDialogueModule module)
                {
                    parentNode.AddModule(module);
                }
            }
            
            public void Clear()
            {
                nodesBuffer.Clear();
            }
            
            public void EndBuildDialogue(IDialogueLookup dialogue)
            {
                graph.ResolveDialogue(dialogue);
            }
        }
    }
}
