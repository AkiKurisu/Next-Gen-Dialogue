using System;
using System.Collections.Generic;
using System.Text;
using Ceres;
using Ceres.Annotations;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action: Build value of string")]
    [CeresLabel("String: Build")]
    [NodeGroup("String")]
    public class BuildString : Action
    {
        public List<SharedString> values;
        
        [ForceShared]
        public SharedString storeResult;
        
        private readonly StringBuilder _stringBuilder = new();
        
        protected override Status OnUpdate()
        {
            _stringBuilder.Clear();
            for (int i = 0; i < values.Count; i++)
            {
                _stringBuilder.Append(values[i].Value);
            }
            storeResult.Value = _stringBuilder.ToString();
            return Status.Success;
        }
    }
}
