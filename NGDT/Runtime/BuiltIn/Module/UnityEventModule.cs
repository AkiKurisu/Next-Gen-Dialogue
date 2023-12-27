
using Kurisu.NGDS;
using UnityEngine;
using UnityEngine.Events;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : UnityEvent Module is used to add UnityEvent callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class UnityEventModule : CustomModule
    {
        [SerializeField, WrapField]
        private UnityEvent unityEvent;
        protected override IDialogueModule GetModule()
        {
            return new NGDS.CallBackModule(unityEvent.Invoke);
        }
    }
}
