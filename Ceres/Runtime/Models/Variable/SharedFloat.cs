using System;
namespace Ceres
{
    [Serializable]
    public class SharedFloat : SharedVariable<float>
    {
        public SharedFloat(float value)
        {
            this.value = value;
        }
        public SharedFloat()
        {

        }
        protected override SharedVariable<float> CloneT()
        {
            return new SharedFloat() { Value = value };
        }
    }
}