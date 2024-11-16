using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Operate float value")]
    [NodeLabel("Math: FloatOperator")]
    [NodeGroup("Math")]
    public class FloatOperator : Action
    {
        private enum Operation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Min,
            Max,
            Modulo
        }
        [SerializeField]
        private SharedFloat float1;
        [SerializeField]
        private SharedFloat float2;
        [SerializeField, ForceShared]
        private SharedFloat storeResult;
        [SerializeField]
        private Operation operation;
        protected override Status OnUpdate()
        {
            switch (operation)
            {
                case Operation.Add:
                    storeResult.Value = float1.Value + float2.Value;
                    break;
                case Operation.Subtract:
                    storeResult.Value = float1.Value - float2.Value;
                    break;
                case Operation.Multiply:
                    storeResult.Value = float1.Value * float2.Value;
                    break;
                case Operation.Divide:
                    storeResult.Value = float1.Value / float2.Value;
                    break;
                case Operation.Min:
                    storeResult.Value = Mathf.Min(float1.Value, float2.Value);
                    break;
                case Operation.Max:
                    storeResult.Value = Mathf.Max(float1.Value, float2.Value);
                    break;
                case Operation.Modulo:
                    storeResult.Value = float1.Value % float2.Value;
                    break;
            }
            return Status.Success;
        }
    }
}