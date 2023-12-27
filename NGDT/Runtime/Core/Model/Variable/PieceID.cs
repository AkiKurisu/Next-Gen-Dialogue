using System;
namespace Kurisu.NGDT
{
    [Serializable]
    public class PieceID : SharedVariable<string>
    {
        public PieceID()
        {
            IsShared = true;
        }
        public override SharedVariable Clone()
        {
            return new PieceID() { Value = value, Name = Name, IsShared = IsShared, IsGlobal = IsGlobal };
        }
    }
}
