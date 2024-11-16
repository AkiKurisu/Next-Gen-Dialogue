using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Set Shared AudioClip Value")]
    [NodeLabel("Audio: SetAudioClip")]
    [NodeGroup("Audio")]
    public class SetAudioClip : Action
    {
        public SharedTObject<AudioClip> source;
        [ForceShared]
        public SharedTObject<AudioClip> storeResult;
        protected override Status OnUpdate()
        {
            storeResult.Value = source.Value;
            return Status.Success;
        }
    }
}