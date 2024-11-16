
using Ceres;
using Ceres.Annotations;
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    [NodeInfo("Module: ScriptableEvent Module is used to add ScriptableEvent callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class ScriptableEventModule : CustomModule
    {
        public Ceres.SharedTObject<ScriptableEvent> scriptableEvent;
        protected override IDialogueModule GetModule()
        {
            return new NGDS.CallBackModule(scriptableEvent.Value.Invoke);
        }
    }
}
