using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Set string value")]
    [CeresLabel("String: Set")]
    [CeresGroup("String")]
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
