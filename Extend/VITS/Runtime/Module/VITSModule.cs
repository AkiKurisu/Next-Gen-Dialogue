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
        private SharedInt characterID;
        public override void Awake()
        {
            base.Awake();
            InitVariable(characterID);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.VITS.VITSModule(characterID.Value);
        }
    }
}
