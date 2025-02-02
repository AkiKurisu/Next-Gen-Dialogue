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
    public abstract class DialogueNode: CeresNode, ILinkedNode, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [HideInGraphEditor, NonSerialized]
        public Action<Status> NotifyEditor;
#endif

        [HideInGraphEditor, SerializeField]
        internal CeresNodeData nodeData;
        
        protected GameObject GameObject { private set; get; }
        
        protected NextGenDialogueComponent Component { private set; get; }
        
        protected DialogueGraph Graph { private set; get; }
        
        public void Initialize(NextGenDialogueComponent component, DialogueGraph graph)
        {
            GameObject = component.gameObject;
            Component = component;
            Graph = graph;
        }

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

        public void OnBeforeSerialize()
        {
            nodeData = NodeData.Clone();
        }

        public void OnAfterDeserialize()
        {
            NodeData = nodeData;
        }
    }
}