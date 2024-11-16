using System.Collections.Generic;
using Ceres;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    public delegate void ResolveDialogueDelegate(IDialogueLookup dialogue);
    [DisallowMultipleComponent]
    public class NextGenDialogueTree : MonoBehaviour, IDialogueTree
    {
        #region  Tree Part
        [HideInInspector, SerializeReference]
        private Root root = new();
        
        Object IDialogueTree.Object => gameObject;
        
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
        
#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        private List<GroupBlockData> blockData = new();
        
        public List<GroupBlockData> BlockData => blockData;
#endif
        public Root Root
        {
            get => root;
        }
        
        public List<SharedVariable> SharedVariables
        {
            get => sharedVariables;
        }
        
        public IDialogueSystem System { get; set; }
        
        private DialogueBuilder builder;
        
        public IDialogueBuilder Builder => builder;
        #endregion
        private void Awake()
        {
            builder = new DialogueBuilder(this);
            foreach (var variable in sharedVariables)
            {
                if (variable is PieceID pieceID) pieceID.Value = global::System.Guid.NewGuid().ToString();
            }
            this.MapGlobal();
            if (externalDialogueTree)
            {
                //Prevent remap for external tree
                if (!externalDialogueTree.IsInitialized)
                    externalDialogueTree.Initialize();
            }
            else
            {
                SharedVariableMapper.Traverse(this);
            }
            root.Run(gameObject, this);
            root.Awake();
        }
        private void Start()
        {
            root.Start();
        }
        /// <summary>
        /// Play the dialogue tree
        /// </summary>
        public void PlayDialogue()
        {
            builder.Clear();
            if (externalDialogueTree)
            {
                externalDialogueTree.Init(gameObject, builder);
                externalDialogueTree.Root.Update();
                return;
            }
            root.Abort();
            root.Update();
        }
        private void ResolveDialogue(IDialogueLookup dialogue)
        {
            System ??= IOCContainer.Resolve<IDialogueSystem>();
            if (System != null)
                System.StartDialogue(dialogue);
            else
            {
                Debug.LogError("No dialogue system registered!");
            }
        }
        private class DialogueBuilder : IDialogueBuilder
        {
            public DialogueBuilder(NextGenDialogueTree tree)
            {
                this.tree = tree;
            }
            private readonly NextGenDialogueTree tree;
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
                    parentNode.AddModule(module);
            }
            public void Clear()
            {
                nodesBuffer.Clear();
            }
            public void EndBuildDialogue(IDialogueLookup dialogue)
            {
                tree.ResolveDialogue(dialogue);
            }
        }
    }
}
