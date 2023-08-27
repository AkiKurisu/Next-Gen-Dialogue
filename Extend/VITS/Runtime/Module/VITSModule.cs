using Kurisu.NGDS;
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
        private int characterID;
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.VITS.VITSModule(characterID);
        }
    }
}
