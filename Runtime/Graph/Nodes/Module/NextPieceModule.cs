using System;
using Ceres.Annotations;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("Next Piece")]
    [NodeInfo("Define the next dialogue piece to play when parent piece has no option.")]
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
