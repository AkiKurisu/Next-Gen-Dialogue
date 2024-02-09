
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    [AkiInfo("Module: ScriptableEvent Module is used to add ScriptableEvent callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class ScriptableEventModule : CustomModule
    {
        public SharedTObject<ScriptableEvent> scriptableEvent;
        protected override IDialogueModule GetModule()
        {
            return new NGDS.CallBackModule(scriptableEvent.Value.Invoke);
        }
    }
}
