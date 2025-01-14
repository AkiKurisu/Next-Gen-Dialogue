using UnityEngine;
using System;
namespace Kurisu.NGDS
{
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "Next Gen Dialogue/ScriptableEvent")]
    public class ScriptableEvent : ScriptableObject
    {
#if UNITY_EDITOR
        [TextArea(0, 3)]
        [Tooltip("Editor only description for developer"), SerializeField]
        internal string devDescription = "";
#endif
        public event Action OnTrigger;
        
        public virtual void Invoke()
        {
            OnTrigger?.Invoke();
        }
    }
}