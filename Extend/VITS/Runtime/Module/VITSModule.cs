using Kurisu.NGDS;
using Kurisu.NGDS.VITS;
using UnityEngine;
namespace Kurisu.NGDT.VITS
{
    [AkiInfo("Module : VITS Module is used to generate audio for dialogue using VITS model.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Piece))]
    [ModuleOf(typeof(Option))]
    public class VITSModule : CustomModule
    {
        [SerializeField]
        private SharedInt characterID;
        [SerializeField]
        private SharedTObject<AudioClip> audioClip;
        public override void Awake()
        {
            base.Awake();
            InitVariable(characterID);
            InitVariable(audioClip);
        }
        protected sealed override IDialogueModule GetModule()
        {
            if (audioClip.Value != null) return new VITSAudioClipModule(audioClip.Value);
            else return new VITSGenerateModule(characterID.Value);
        }
    }
}
