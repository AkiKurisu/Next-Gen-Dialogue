using UnityEngine;
using System;
using UnityEngine.Serialization;
namespace Kurisu.NGDS
{
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "Next Gen Dialogue/ScriptableEvent")]
    public class ScriptableEvent : ScriptableObject
    {
#if UNITY_EDITOR
        [FormerlySerializedAs("_devDescription")]
        [TextArea(0, 3)]
        [Tooltip("Editor Only Description For Developer."), SerializeField]
        private string devDescription = "";
#endif
        public event Action OnTrigger;
        
        public virtual void Invoke()
        {
            OnTrigger?.Invoke();
        }
    }
}