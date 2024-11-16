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
    public abstract class NodeBehavior: CeresNode
    {
#if UNITY_EDITOR
        [HideInEditorWindow, NonSerialized]
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
    }
}