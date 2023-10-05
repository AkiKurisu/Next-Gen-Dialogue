using UnityEngine;
using System.Collections.Generic;
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    [CreateAssetMenu(fileName = "NextGenDialogueTreeSO", menuName = "Next Gen Dialogue/NextGenDialogueTreeSO")]
    public class NextGenDialogueTreeSO : ScriptableObject, IDialogueTree
    {
        [SerializeReference, HideInInspector]
        protected Root root = new();
        Object IDialogueTree.Object => this;
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
#if UNITY_EDITOR
        public IDialogueTree ExternalBehaviorTree => null;
        [SerializeField, HideInInspector]
        private List<GroupBlockData> blockData = new();
        public List<GroupBlockData> BlockData { get => blockData; set => blockData = value; }
        [Multiline, SerializeField]
        private string Description;
#endif
        [HideInInspector]
        [SerializeReference]
        protected List<SharedVariable> sharedVariables = new();
        public IDialogueBuilder Builder { get; private set; }
        public IDialogueSystem System { get; set; }
        /// <summary>
        /// Bind GameObject and Init behaviorTree through Awake and Start method
        /// <param name="gameObject">bind gameObject</param>
        /// <param name="dialogueBuilder">runtime builder</param>
        /// </summary>
        public void Init(GameObject gameObject, IDialogueBuilder dialogueBuilder)
        {
            Builder = dialogueBuilder;
            GenerateID();
            root.Abort();
            root.Run(gameObject, this);
            root.Awake();
            root.Start();
        }
        public void Deserialize(string serializedData)
        {
            var template = SerializationUtility.DeserializeTree(serializedData);
            if (template == null) return;
            root = template.Root;
            sharedVariables = new List<SharedVariable>(template.Variables);
        }
        private void GenerateID()
        {
            foreach (var variable in SharedVariables)
            {
                if (variable is PieceID pieceID) pieceID.Value = global::System.Guid.NewGuid().ToString();
            }
        }
    }
}
