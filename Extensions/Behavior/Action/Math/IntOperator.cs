using Ceres;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Action: Operate int value")]
    [CeresLabel("Math: IntOperator")]
    [CeresGroup("Math")]
    public class IntOperator : Action
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
        private SharedInt int1;
        [SerializeField]
        private SharedInt int2;
        [SerializeField, ForceShared]
        private SharedInt storeResult;
        [SerializeField]
        private Operation operation;
        protected override Status OnUpdate()
        {
            switch (operation)
            {
                case Operation.Add:
                    storeResult.Value = int1.Value + int2.Value;
                    break;
                case Operation.Subtract:
                    storeResult.Value = int1.Value - int2.Value;
                    break;
                case Operation.Multiply:
                    storeResult.Value = int1.Value * int2.Value;
                    break;
                case Operation.Divide:
                    storeResult.Value = int1.Value / int2.Value;
                    break;
                case Operation.Min:
                    storeResult.Value = Mathf.Min(int1.Value, int2.Value);
                    break;
                case Operation.Max:
                    storeResult.Value = Mathf.Max(int1.Value, int2.Value);
                    break;
                case Operation.Modulo:
                    storeResult.Value = int1.Value % int2.Value;
                    break;
            }
            return Status.Success;
        }
    }
}