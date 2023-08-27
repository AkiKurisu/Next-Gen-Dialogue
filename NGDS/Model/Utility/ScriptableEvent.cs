using UnityEngine;
using System;
namespace Kurisu.NGDS
{
    [CreateAssetMenu(fileName = "ScriptableEvent", menuName = "Next Gen Dialogue/ScriptableEvent")]
    public class ScriptableEvent : ScriptableObject
    {
#if UNITY_EDITOR
        [TextArea(0, 3)]
        [Tooltip("Editor Only Description For Developer.")]
        public string _devDescription = "";
#endif
        public event Action OnTrigger;
        public virtual void Invoke()
        {
            OnTrigger?.Invoke();
        }
    }
}