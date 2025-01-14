using System;
using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [Serializable]
    [NodeInfo("Action : Operate Vector3 value")]
    [CeresLabel("Vector3 : Operator")]
    [CeresGroup("Vector3")]
    public class Vector3Operator : Action
    {

        public enum Operation
        {
            Add,
            Subtract,
            Scale
        }
        
        public Operation operation;
        
        public SharedVector3 firstVector3;
        
        public SharedVector3 secondVector3;
        
        [ForceShared]
        public SharedVector3 storeResult;
        
        protected override Status OnUpdate()
        {
            storeResult.Value = operation switch
            {
                Operation.Add => firstVector3.Value + secondVector3.Value,
                Operation.Subtract => firstVector3.Value - secondVector3.Value,
                Operation.Scale => Vector3.Scale(firstVector3.Value, secondVector3.Value),
                _ => storeResult.Value
            };
            return Status.Success;
        }
    }
}