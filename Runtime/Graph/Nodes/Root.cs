using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Annotations;
using Ceres.Graph;
using Cysharp.Threading.Tasks;
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
        
        protected sealed override Status OnUpdate()
        {
            DialogueGraphTracker.GetActiveTracker().OnDialogueUpdate(this).Forget();

            var dialogue = GetActiveDialogue();
            return dialogue?.Update(Children.OfType<Piece>()) ?? Status.Failure;
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