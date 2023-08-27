using System.Collections.Generic;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    public delegate void ResolveDialogueDelegate(IProvideDialogue dialogue);
    [DisallowMultipleComponent]
    public class NextGenDialogueTree : MonoBehaviour, IDialogueTree
    {
        #region  Tree Part
        [HideInInspector, SerializeReference]
        private Root root = new();
        Object IDialogueTree._Object => gameObject;
        [HideInInspector]
        [SerializeReference]
        private List<SharedVariable> sharedVariables = new();
        [SerializeField, Tooltip("Replace the dialogue tree in the component with the external dialogue tree," +
        " which will overwrite the dialogue tree in the component when saving")]
        private NextGenDialogueTreeSO externalDialogueTree;
        /// <summary>
        /// Overwrite external dialogueTreeSO to use external data, and leave null to use embedded data.
        /// </summary>
        /// <value></value>
        public NextGenDialogueTreeSO ExternalData { get => externalDialogueTree; set => externalDialogueTree = value; }
#if UNITY_EDITOR
        public IDialogueTree ExternalBehaviorTree => externalDialogueTree;
        [SerializeField, HideInInspector]
        private List<GroupBlockData> blockData = new();
        public List<GroupBlockData> BlockData { get => blockData; set => blockData = value; }
#endif
        public Root Root
        {
            get => root;
#if UNITY_EDITOR
            set => root = value;
#endif
        }
        public List<SharedVariable> SharedVariables
        {
            get => sharedVariables;
#if UNITY_EDITOR
            set => sharedVariables = value;
#endif
        }
        public IDialogueSystem System { get; set; }
        private DialogueBuilder builder;
        public IDialogueBuilder Builder => builder;
        #endregion
        private void Awake()
        {
            builder = new DialogueBuilder(ResolveDialogue);
            foreach (var variable in sharedVariables)
            {
                if (variable is PieceID pieceID) pieceID.Value = global::System.Guid.NewGuid().ToString();
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
            builder.ClearBuffer();
            if (externalDialogueTree)
            {
                externalDialogueTree.Init(gameObject, builder);
                externalDialogueTree.Root.Update();
                return;
            }
            root.Abort();
            root.Update();
        }
        private void ResolveDialogue(IProvideDialogue dialogue)
        {
            System ??= IOCContainer.Resolve<IDialogueSystem>();
            if (System != null)
                System.StartDialogue(dialogue);
            else
            {
                Debug.Log("No dialogue system registered !");
            }
        }
    }
}
