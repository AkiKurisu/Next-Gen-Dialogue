using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action : Set Shared AudioClip Value")]
    [AkiLabel("Audio : SetAudioClip")]
    [AkiGroup("Audio")]
    public class SetAudioClip : Action
    {
        [SerializeField]
        private SharedTObject<AudioClip> source;
        [SerializeField, ForceShared]
        private SharedTObject<AudioClip> storeResult;
        public override void Awake()
        {
            InitVariable(source);
            InitVariable(storeResult);
        }
        protected override Status OnUpdate()
        {
            storeResult.Value = source.Value;
            return Status.Success;
        }
    }
}