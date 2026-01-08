using System;
using Ceres.Annotations;
using Ceres.Graph;
using UnityEngine;

namespace NextGenDialogue.Graph.VITS
{
    [Serializable]
    [CeresLabel("VITS Voice")]
    [NodeInfo("Provide or generate voice for parent container using VITS model.")]
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
            if (audioClip.Value) return new NextGenDialogue.VITS.VITSModule(audioClip.Value);
            return new NextGenDialogue.VITS.VITSModule(characterID.Value, noTranslation);
        }
    }
}
