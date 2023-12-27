using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Module : Next Piece Module is used to play target dialogue piece automatically" +
    " when parent piece is completed and have no option.")]
    [ModuleOf(typeof(Piece))]
    public class NextPieceModule : CustomModule
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool useReference;
#endif
        [SerializeField, AkiLabel("Next ID"), Tooltip("The next dialogue piece's PieceID"), ReferencePieceID]
        private PieceID nextID;
        public PieceID NextID
        {
            get => nextID;
            set => nextID = value;
        }
        public override void Awake()
        {
            InitVariable(nextID);
        }
        protected sealed override IDialogueModule GetModule()
        {
            return new NGDS.NextPieceModule(nextID.Value);
        }
    }
}
