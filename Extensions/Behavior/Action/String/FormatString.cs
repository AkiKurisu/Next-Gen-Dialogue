using System;
using System.Collections.Generic;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable, NodeInfo("Action: Format value of string"), CeresLabel("String: Format"), NodeGroup("String")]
    public class FormatString : Action
    {
        public SharedString format;
        
        public List<SharedString> parameters;
        
        [ForceShared]
        public SharedString storeResult;
        
        private string[] _parameterValues;
        
        public override void Awake()
        {
            _parameterValues = new string[parameters.Count];
        }
        protected override Status OnUpdate()
        {
            for (int i = 0; i < _parameterValues.Length; ++i)
            {
                _parameterValues[i] = parameters[i].Value;
            }
            try
            {
                storeResult.Value = string.Format(format.Value, _parameterValues);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return Status.Success;
        }
    }
}
