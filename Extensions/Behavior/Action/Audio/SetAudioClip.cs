using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Set Shared AudioClip Value")]
    [CeresLabel("Audio: SetAudioClip")]
    [CeresGroup("Audio")]
    public class SetAudioClip : Action
    {
        public SharedUObject<AudioClip> source;
        
        [ForceShared]
        public SharedUObject<AudioClip> storeResult;
        
        protected override Status OnUpdate()
        {
            storeResult.Value = source.Value;
            return Status.Success;
        }
    }
}