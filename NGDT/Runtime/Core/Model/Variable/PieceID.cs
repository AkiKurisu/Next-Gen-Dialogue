namespace Kurisu.NGDT
{
    [System.Serializable]
    public class PieceID : SharedVariable<string>, IBindableVariable<PieceID>
    {
        public PieceID()
        {
            IsShared = true;
        }

        public void Bind(PieceID other)
        {
            base.Bind(other);
        }

        public override object Clone()
        {
            return new PieceID() { Value = value, Name = Name, IsShared = IsShared };
        }
    }
}
