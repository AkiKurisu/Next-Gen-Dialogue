using System;
namespace Kurisu.NGDT
{
    [Serializable]
    public class SharedFloat : SharedVariable<float>, IBindableVariable<SharedFloat>
    {
        public SharedFloat(float value)
        {
            this.value = value;
        }
        public SharedFloat()
        {

        }
        public override object Clone()
        {
            return new SharedFloat() { Value = this.value, Name = this.Name, IsShared = this.IsShared };
        }
        public void Bind(SharedFloat other)
        {
            base.Bind(other);
        }
    }
}