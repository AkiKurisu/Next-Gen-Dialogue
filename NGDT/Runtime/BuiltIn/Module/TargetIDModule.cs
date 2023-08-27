using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : TargetID Module is used to modify option's target dialogue piece.")]
    [ModuleOf(typeof(Option))]
    public class TargetIDModule : CustomModule
    {
        [SerializeField, AkiLabel("Target ID"), Tooltip("The target dialogue piece's PieceID"), ReferencePieceID]
        private PieceID targetID;
        public override void Awake()
        {
            InitVariable(targetID);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.TargetIDModule(targetID.Value);
        }
    }
}
