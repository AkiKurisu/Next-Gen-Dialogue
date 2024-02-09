using UnityEngine;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Set string value")]
    [AkiLabel("String: Set")]
    [AkiGroup("String")]
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
