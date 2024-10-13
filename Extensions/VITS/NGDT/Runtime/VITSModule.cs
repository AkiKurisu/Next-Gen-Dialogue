using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT.VITS
{
    [AkiInfo("Module : VITS Module is used to generate audio for dialogue using VITS model.")]
    [AkiGroup("AIGC")]
    [ModuleOf(typeof(Piece), true)]
    [ModuleOf(typeof(Option))]
    public class VITSModule : CustomModule
    {
        public SharedInt characterID;
        public SharedTObject<AudioClip> audioClip;
        [Setting, Tooltip("Set to disable translation for this module")]
        public bool noTranslation;
        protected sealed override IDialogueModule GetModule()
        {
            if (audioClip.Value != null) return new NGDS.VITS.VITSModule(audioClip.Value);
            else return new NGDS.VITS.VITSModule(characterID.Value, noTranslation);
        }
    }
}
