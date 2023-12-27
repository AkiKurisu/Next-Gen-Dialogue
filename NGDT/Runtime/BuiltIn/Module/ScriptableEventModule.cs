
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : ScriptableEvent Module is used to add ScriptableEvent callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class ScriptableEventModule : CustomModule
    {
        [SerializeField]
        private SharedTObject<ScriptableEvent> scriptableEvent;
        public override void Awake()
        {
            InitVariable(scriptableEvent);
        }
        protected override IDialogueModule GetModule()
        {
            return new NGDS.CallBackModule(scriptableEvent.Value.Invoke);
        }
    }
}
