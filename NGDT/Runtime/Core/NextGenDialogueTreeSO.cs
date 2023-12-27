using UnityEngine;
using System.Collections.Generic;
using Kurisu.NGDS;
#if NGDT_REFLECTION&&UNITY_EDITOR
using UnityEditor;
#endif
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
        }
        public List<SharedVariable> SharedVariables => sharedVariables;
#if UNITY_EDITOR
        [SerializeField, HideInInspector]
        private List<GroupBlockData> blockData = new();
        public List<GroupBlockData> BlockData => blockData;
        [Multiline, SerializeField]
        private string Description;
#endif
        [HideInInspector]
        [SerializeReference]
        protected List<SharedVariable> sharedVariables = new();
        public IDialogueBuilder Builder { get; private set; }
        public IDialogueSystem System { get; set; }
        /// <summary>
        /// Whether dialogueTreeSO is initialized
        /// </summary>
        /// <value></value>
#if NGDT_REFLECTION
        public bool IsInitialized { get; private set; }
#else
        public bool IsInitialized => true;
#endif
        /// <summary>
        /// Bind GameObject and Init behaviorTree through Awake and Start method
        /// <param name="gameObject">bind gameObject</param>
        /// <param name="dialogueBuilder">runtime builder</param>
        /// </summary>
        public void Init(GameObject gameObject, IDialogueBuilder dialogueBuilder)
        {
#if NGDT_REFLECTION
#if !UNITY_EDITOR
            if (!IsInitialized)
#endif
            {
                Initialize();
            }
#endif
            this.MapGlobal();
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
#if UNITY_EDITOR
            blockData = new List<GroupBlockData>(template.BlockData);
#endif
            Initialize();
        }
        private void GenerateID()
        {
            foreach (var variable in SharedVariables)
            {
                if (variable is PieceID pieceID) pieceID.Value = global::System.Guid.NewGuid().ToString();
            }
        }
#if NGDT_REFLECTION
        /// <summary>
        /// This will be called when the object is loaded for the first time when entering PlayMode
        /// Currently we only need to map variables once per scriptableObject
        /// </summary>
        private void Awake()
        {
            if (!IsInitialized)
            {
                Initialize();
            }
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayStateChange;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayStateChange;
        }

        private void OnPlayStateChange(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.ExitingPlayMode)
                IsInitialized = false;
        }
#endif
#endif
        /// <summary>
        /// Traverse behaviors and bind shared variables
        /// </summary>
        public void Initialize()
        {
#if NGDT_REFLECTION
            IsInitialized = true;
            SharedVariableMapper.Traverse(this);
#endif
        }
    }
}
