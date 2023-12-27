using System;
namespace Kurisu.NGDT
{
    [Serializable]
    public class SharedInt : SharedVariable<int>
    {
        public SharedInt(int value)
        {
            this.value = value;
        }
        public SharedInt()
        {

        }
        public override SharedVariable Clone()
        {
            return new SharedInt() { Value = value, Name = Name, IsShared = IsShared, IsGlobal = IsGlobal };
        }
    }
}