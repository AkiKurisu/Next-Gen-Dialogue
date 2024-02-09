using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Set Shared AudioClip Value")]
    [AkiLabel("Audio: SetAudioClip")]
    [AkiGroup("Audio")]
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