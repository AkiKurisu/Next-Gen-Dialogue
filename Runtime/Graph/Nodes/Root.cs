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
        [SerializeReference]
        private List<DialogueNode> children = new();
        
        public List<DialogueNode> Children => children;
        
#if UNITY_EDITOR
        [HideInGraphEditor]
        public Action UpdateEditor;
#endif
        
        protected sealed override Status OnUpdate()
        {
#if UNITY_EDITOR
            UpdateEditor?.Invoke();
#endif

            var dialogue = GetActiveDialogue();
            if (dialogue == null) return Status.Failure;
            return dialogue.Update(Children.OfType<Piece>());
        }
        
        /// <summary>
        /// Get active dialogue
        /// </summary>
        /// <returns></returns>
        public Dialogue GetActiveDialogue()
        {
            foreach (var child in Children)
            {
                if (child is Dialogue dialogue) return dialogue;
            }
            return null;
        }
        
        public sealed override void AddChild(CeresNode inChild)
        {
            children.Add((DialogueNode)inChild);
        }
        
        public sealed override CeresNode GetChildAt(int index)
        {
            return children[index];
        }
        
        public sealed override int GetChildrenCount()
        {
            return children.Count;
        }
        
        public sealed override void ClearChildren()
        {
            children.Clear();
        }
    }
}