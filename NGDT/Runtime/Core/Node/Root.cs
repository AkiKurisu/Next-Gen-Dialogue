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
        [HideInEditorWindow]
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
            for (int i = 0; i < children.Count; i++)
            {
                //Skip inactive dialogue
                if (children[i] is not Dialogue)
                    children[i].Run(GameObject, Graph);
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
            // Only update main dialogue
            if (child == null) return Status.Failure;
            return GetActiveDialogue().Update(Children.OfType<Piece>());
        }

        internal void Abort()
        {
            GetActiveDialogue().Abort();
            for (int i = 0; i < children.Count; i++)
            {
                // Skip inactive dialogue
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
        
        public sealed override void AddChild(CeresNode inChild)
        {
            if (child == null)
            {
                child = inChild as NodeBehavior;
                return;
            }
            children.Add(inChild as NodeBehavior);
        }
        
        public sealed override CeresNode GetChildAt(int index)
        {
            if (index == 0) return child;
            return children[index - 1];
        }
        
        public sealed override int GetChildrenCount()
        {
            if (child == null) return 0;
            return children.Count + 1;
        }
        
        public sealed override void ClearChildren()
        {
            child = null;
            children.Clear();
        }
        
        public sealed override void SetChildren(CeresNode[] inChildren)
        {
            children.Clear();
            child = null;
            if(inChildren.Length <= 0 ) return;
            foreach (var child in inChildren)
            {
                children.Add(child as NodeBehavior);
            }
            child = children[0];
            children.RemoveAt(0);
        }
        
        public sealed override CeresNode[] GetChildren()
        {
            var list = new List<CeresNode>();
            list.Add(child);
            list.AddRange(children);
            return children.ToArray();
        }
    }
}