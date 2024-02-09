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
        public SharedInt characterID;
        public SharedTObject<AudioClip> audioClip;
        [Setting, Tooltip("Set to disable translation for this module")]
        public bool noTranslation;
        protected sealed override IDialogueModule GetModule()
        {
            if (audioClip.Value != null) return new VITSAudioClipModule(audioClip.Value);
            else return new VITSGenerateModule(characterID.Value, noTranslation);
        }
    }
}
