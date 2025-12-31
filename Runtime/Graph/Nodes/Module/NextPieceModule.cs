using System;
using Ceres.Annotations;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Next Piece")]
    [NodeInfo("Module: Next Piece is used to play target dialogue piece automatically" +
    " when parent piece is completed and have no option.")]
    [ModuleOf(typeof(Piece))]
    public class NextPieceModule : CustomModule
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool useReference;
#endif
        
        [CeresLabel("Next ID"), Tooltip("The next dialogue piece's PieceID"), ReferencePieceID]
        public PieceID nextID;
        
        protected sealed override IDialogueModule GetModule()
        {
            return new NextGenDialogue.NextPieceModule(nextID.Name);
        }
    }
}
