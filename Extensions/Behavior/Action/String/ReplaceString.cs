using System;
using Ceres;
using Ceres.Annotations;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Replace value of string")]
    [CeresLabel("String: Replace")]
    [CeresGroup("String")]
    public class ReplaceString : Action
    {
        public SharedString target;
        
        public SharedString replaceFrom;
        
        public SharedString replaceTo;
        
        [ForceShared]
        public SharedString storeResult;
        
        protected override Status OnUpdate()
        {
            storeResult.Value = target.Value.Replace(replaceFrom.Value, replaceTo.Value);
            return Status.Success;
        }
    }
}
