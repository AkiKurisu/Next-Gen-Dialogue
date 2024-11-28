using System;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;
namespace Kurisu.NGDT
{
    public enum Status
    {
        Success,
        Failure
    }
    /// <summary>
    /// Base class for dialogue graph node
    /// </summary>
    [Serializable]
    public abstract class NodeBehavior: CeresNode, ILinkedNode
    {
#if UNITY_EDITOR
        [HideInGraphEditor, NonSerialized]
        public Action<Status> NotifyEditor;
#endif

        protected GameObject GameObject { private set; get; }
        protected DialogueGraph Graph { private set; get; }
        public void Run(GameObject attachedObject, DialogueGraph graph)
        {
            GameObject = attachedObject;
            Graph = graph;
            OnRun();
        }

        protected abstract void OnRun();

        public virtual void Awake() { }

        public virtual void Start() { }

        public Status Update()
        {
            var status = OnUpdate();

#if UNITY_EDITOR
            NotifyEditor?.Invoke(status);
#endif
            return status;
        }

        protected abstract Status OnUpdate();

        /// <summary>
        /// Get child not at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual CeresNode GetChildAt(int index)
        {
            return null;
        }

        /// <summary>
        /// Add new child node
        /// </summary>
        /// <param name="node"></param>
        public virtual void AddChild(CeresNode node)
        {
            
        }

        /// <summary>
        /// Get child node count
        /// </summary>
        /// <returns></returns>
        public virtual int GetChildrenCount()
        {
            return 0;
        }

        /// <summary>
        /// Clear all child nodes
        /// </summary>
        public virtual void ClearChildren()
        {
            
        }

        /// <summary>
        ///  Get all child nodes
        /// </summary>
        /// <returns></returns>
        public virtual CeresNode[] GetChildren()
        {
            return Array.Empty<CeresNode>();
        }

        /// <summary>
        /// Set child nodes
        /// </summary>
        /// <param name="children"></param>
        public virtual void SetChildren(CeresNode[] children)
        {
            
        }
    }
}