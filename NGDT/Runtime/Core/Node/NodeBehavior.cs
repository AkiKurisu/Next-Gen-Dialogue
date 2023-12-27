using System;
using UnityEngine;
namespace Kurisu.NGDT
{
    public enum Status
    {
        Success,
        Failure
    }
    [Serializable]
    public abstract class NodeBehavior
    {

#if UNITY_EDITOR
        [HideInEditorWindow]
        public Rect graphPosition = new(400, 300, 100, 100);

        [HideInEditorWindow]
        public string description;

        [HideInEditorWindow, NonSerialized]
        public Action<Status> NotifyEditor;
        [SerializeField, HideInEditorWindow]
        private string guid;
        public string GUID { get => guid; set => guid = value; }
#endif

        protected GameObject GameObject { private set; get; }
        protected IDialogueTree Tree { private set; get; }
        public void Run(GameObject attachedObject, IDialogueTree attachedTree)
        {
            GameObject = attachedObject;
            Tree = attachedTree;
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
        protected void InitVariable(SharedVariable sharedVariable)
        {
            //Skip init variable if use reflection runtime
#if !NGDT_REFLECTION
            sharedVariable.MapToInternal(Tree);
#endif
        }
    }
}