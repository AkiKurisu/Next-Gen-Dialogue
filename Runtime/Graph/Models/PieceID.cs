using System;
using Ceres.Graph;
using R3;

namespace NextGenDialogue.Graph
{
    [Serializable]
    public class PieceID : SharedVariable<Unit>
    {
        public PieceID()
        {
            IsShared = true;
            IsExposed = false;
        }
        
        protected override SharedVariable<Unit> CloneT()
        {
            return new PieceID { Value = value };
        }
    }
}
