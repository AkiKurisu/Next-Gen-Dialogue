using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Play audioClip on target audioSource")]
    [AkiLabel("Audio: PlayAudioClip")]
    [AkiGroup("Audio")]
    public class PlayAudioClip : Action
    {
        public SharedTObject<AudioClip> audioClip;
        public SharedTObject<AudioSource> audioSource;
        protected override Status OnUpdate()
        {
            if (audioSource.Value != null)
            {
                audioSource.Value.clip = audioClip.Value;
                audioSource.Value.Play();
            }
            return Status.Success;
        }
    }
}
