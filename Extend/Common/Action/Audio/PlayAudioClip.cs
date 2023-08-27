using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action : Play audioClip on target audioSource")]
    [AkiLabel("Audio:PlayAudioClip")]
    [AkiGroup("Audio")]
    public class PlayAudioClip : Action
    {
        [SerializeField]
        private SharedTObject<AudioClip> audioClip;
        [SerializeField]
        private SharedTObject<AudioSource> audioSource;
        public override void Awake()
        {
            InitVariable(audioClip);
            InitVariable(audioSource);
        }
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
