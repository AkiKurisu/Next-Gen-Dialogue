using System;
using Ceres.Annotations;
using Ceres.Graph;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT.VITS
{
    [Serializable]
    [CeresLabel("VITS Voice")]
    [NodeInfo("Module : VITS is used to generate audio for dialogue using VITS model.")]
    [CeresGroup("AIGC")]
    [ModuleOf(typeof(Piece), true)]
    [ModuleOf(typeof(Option))]
    public class VITSModule : CustomModule
    {
        public SharedInt characterID;
        
        public SharedUObject<AudioClip> audioClip;
        
        [Setting, Tooltip("Set to disable translation for this module")]
        public bool noTranslation;
        
        protected sealed override IDialogueModule GetModule()
        {
            if (audioClip.Value != null) return new NGDS.VITS.VITSModule(audioClip.Value);
            return new NGDS.VITS.VITSModule(characterID.Value, noTranslation);
        }
    }
}
