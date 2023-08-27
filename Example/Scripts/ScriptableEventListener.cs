using UnityEngine;
namespace Kurisu.NGDS.Example
{
    public class ScriptableEventListener : MonoBehaviour
    {
        [SerializeField]
        private ScriptableEvent Event;
        [SerializeField]
        private GameObject Object;
        void Start()
        {
            Event.OnTrigger += ShowObject;
        }
        private void OnDestroy()
        {
            Event.OnTrigger -= ShowObject;
        }

        private void ShowObject()
        {
            Object.SetActive(true);
        }
    }
}