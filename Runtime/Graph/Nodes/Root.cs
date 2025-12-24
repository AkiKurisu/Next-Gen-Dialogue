using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    /// <summary>
    /// Entry point of the dialogue tree
    /// </summary>
    [Serializable]
    [NodeInfo("Root : The root of dialogue tree, you can not delete it.")]
    public class Root : DialogueNode
    {
        /* Main dialogue */
        [SerializeReference]
        private DialogueNode child;
        
        /* Unused dialogues */
        [SerializeReference]
        private List<DialogueNode> children = new();
        
        public List<DialogueNode> Children => children;
        
#if UNITY_EDITOR
        [HideInGraphEditor]
        public Action UpdateEditor;
#endif
        
        public DialogueNode Child
        {
            get => child;
            set => child = value;
        }
        
        public override void Awake()
        {
            child?.Awake();
            foreach (var node in children)
            {
                // Skip inactive dialogue
                if (node is not Dialogue)
                {
                    node.Awake();
                }
            }
        }

        public override void Start()
        {
            child?.Start();
            foreach (var node in children)
            {
                // Skip inactive dialogue
                if (node is not Dialogue)
                {
                    node.Start();
                }
            }
        }
        
        protected sealed override Status OnUpdate()
        {
#if UNITY_EDITOR
            UpdateEditor?.Invoke();
#endif
            // Only update main dialogue
            if (child == null) return Status.Failure;
            return GetActiveDialogue().Update(Children.OfType<Piece>());
        }
        
        /// <summary>
        /// Get active dialogue
        /// </summary>
        /// <returns></returns>
        public Dialogue GetActiveDialogue()
        {
            return (Dialogue)child;
        }
        
        /// <summary>
        /// Add child node to root's <see cref="children"/> set main dialogue if <see cref="Child"/> not exist
        /// </summary>
        /// <param name="inChild"></param>
        public sealed override void AddChild(CeresNode inChild)
        {
            if (child == null)
            {
                child = (DialogueNode)inChild;
                return;
            }
            children.Add((DialogueNode)inChild);
        }
        
        public sealed override CeresNode GetChildAt(int index)
        {
            return index == 0 ? child : children[index - 1];
        }
        
        public sealed override int GetChildrenCount()
        {
            return children.Count + (child == null ? 0 : 1);
        }
        
        public sealed override void ClearChildren()
        {
            child = null;
            children.Clear();
        }
    }
}