using Ceres;
using Ceres.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set string value")]
    [CeresLabel("String: Set")]
    [NodeGroup("String")]
    public class SetString : Action
    {
        [Multiline, TranslateEntry, FormerlySerializedAs("value")]
        public SharedString stringValue;
        [ForceShared]
        public SharedString storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = stringValue.Value;
            return Status.Success;
        }
    }
}
