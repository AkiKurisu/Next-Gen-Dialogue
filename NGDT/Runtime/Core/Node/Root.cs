using System;
using System.Collections.Generic;
using System.Linq;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Entry point of the dialogue tree
    /// </summary>
    [Serializable]
    [NodeInfo("Root : The root of dialogue tree, you can not delete it.")]
    public class Root : NodeBehavior
    {
        /* Main dialogue */
        [SerializeReference]
        private NodeBehavior child;
        
        /* Unused dialogues */
        [SerializeReference]
        private List<NodeBehavior> children = new();
        
        public List<NodeBehavior> Children => children;
        
#if UNITY_EDITOR
        [HideInGraphEditor]
        public System.Action UpdateEditor;
#endif
        
        public NodeBehavior Child
        {
            get => child;
            set => child = value;
        }

        protected sealed override void OnRun()
        {
            child?.Run(GameObject, Graph);
            foreach (var node in children)
            {
                // Skip inactive dialogue
                if (node is not Dialogue)
                {
                    node.Run(GameObject, Graph);
                }
            }
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

        internal void Abort()
        {
            GetActiveDialogue().Abort();
            foreach (var node in children)
            {
                // Skip inactive dialogue
                if (node is Container container and not Dialogue)
                {
                    container.Abort();
                }
            }
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
                child = (NodeBehavior)inChild;
                return;
            }
            children.Add((NodeBehavior)inChild);
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