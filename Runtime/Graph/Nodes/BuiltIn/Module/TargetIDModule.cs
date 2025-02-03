using System;
using Ceres.Annotations;
using NextGenDialogue;
using UnityEngine;
namespace NextGenDialogue.Graph
{
    [Serializable]
    [CeresLabel("TargetID")]
    [NodeInfo("Module: TargetID is used to definite option's target dialogue piece id.")]
    [ModuleOf(typeof(Option))]
    public class TargetIDModule : CustomModule
    {
#if UNITY_EDITOR
        [SerializeField]
        private bool useReference;
#endif
        
        [CeresLabel("Target ID"), Tooltip("The target dialogue piece's PieceID"), ReferencePieceID]
        public PieceID targetID;
        
        protected sealed override IDialogueModule GetModule()
        {
            return new NextGenDialogue.TargetIDModule(targetID.Value);
        }
        
        public TargetIDModule() { }
        
        public TargetIDModule(string targetID)
        {
            this.targetID = new PieceID() { Name = targetID };
        }
    }
}
