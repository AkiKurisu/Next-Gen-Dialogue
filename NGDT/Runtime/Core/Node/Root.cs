using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Entry point of the dialogue tree
    /// </summary>
    [AkiInfo("Root : The root of dialogue tree, you can not delate it.")]
    public class Root : NodeBehavior, IIterable
    {
        [SerializeReference]
        private NodeBehavior child;
        [SerializeReference]
        private List<NodeBehavior> children = new();
        public List<NodeBehavior> Children => children;
#if UNITY_EDITOR
        [HideInEditorWindow]
        public System.Action UpdateEditor;
        public void AddChild(NodeBehavior child)
        {
            children.Add(child);
        }
#endif
        public NodeBehavior Child
        {
            get => child;
#if UNITY_EDITOR
            set => child = value;
#endif
        }

        protected sealed override void OnRun()
        {
            child?.Run(GameObject, Tree);
            for (int i = 0; i < children.Count; i++)
            {
                //Skip inactive dialogue
                if (children[i] is not Dialogue)
                    children[i].Run(GameObject, Tree);
            }
        }

        public override void Awake()
        {
            child?.Awake();
            for (int i = 0; i < children.Count; i++)
            {
                //Skip inactive dialogue
                if (children[i] is not Dialogue)
                    children[i].Awake();
            }
        }

        public override void Start()
        {
            child?.Start();
            for (int i = 0; i < children.Count; i++)
            {
                //Skip inactive dialogue
                if (children[i] is not Dialogue)
                    children[i].Start();
            }
        }
        protected sealed override Status OnUpdate()
        {
#if UNITY_EDITOR
            UpdateEditor?.Invoke();
#endif
            //Only update Child Dialogue
            if (child == null) return Status.Failure;
            return GetActiveDialogue().Update(Children.OfType<Piece>());
        }

        internal void Abort()
        {
            GetActiveDialogue().Abort();
            for (int i = 0; i < children.Count; i++)
            {
                //Skip inactive dialogue
                if (children[i] is Container container and not Dialogue)
                    container.Abort();
            }
        }
        /// <summary>
        /// Get active dialogue
        /// </summary>
        /// <returns></returns>
        public Dialogue GetActiveDialogue()
        {
            return child as Dialogue;
        }

        public NodeBehavior GetChildAt(int index)
        {
            if (index == 0) return child;
            return children[index - 1];
        }

        public int GetChildCount()
        {
            if (child == null) return 0;
            return children.Count + 1;
        }
    }
}