
using System;
using Ceres;
using Ceres.Annotations;
using Ceres.Graph;
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Module: ScriptableEvent Module is used to add ScriptableEvent callBack for dialogue option")]
    [ModuleOf(typeof(Option))]
    public class ScriptableEventModule : CustomModule
    {
        public SharedUObject<ScriptableEvent> scriptableEvent;
        
        protected override IDialogueModule GetModule()
        {
            return new NGDS.CallBackModule(scriptableEvent.Value.Invoke);
        }
    }
}
