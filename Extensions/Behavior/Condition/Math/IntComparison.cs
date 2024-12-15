using Ceres;
using Ceres.Annotations;

namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Condition: Compare Int values, if the conditions are met, return Status.Success, otherwise return Status.Failure")]
    [CeresLabel("Math: IntComparison")]
    [NodeGroup("Math")]
    public class IntComparison : Conditional
    {
        public enum Operation
        {
            LessThan,
            LessThanOrEqualTo,
            EqualTo,
            NotEqualTo,
            GreaterThanOrEqualTo,
            GreaterThan
        }
        public SharedInt int1;
        public SharedInt int2;
        public Operation operation;
        protected override Status IsUpdatable()
        {
            return operation switch
            {
                Operation.LessThan => int1.Value < int2.Value ? Status.Success : Status.Failure,
                Operation.LessThanOrEqualTo => int1.Value <= int2.Value ? Status.Success : Status.Failure,
                Operation.EqualTo => int1.Value == int2.Value ? Status.Success : Status.Failure,
                Operation.NotEqualTo => int1.Value != int2.Value ? Status.Success : Status.Failure,
                Operation.GreaterThanOrEqualTo => int1.Value >= int2.Value ? Status.Success : Status.Failure,
                Operation.GreaterThan => int1.Value > int2.Value ? Status.Success : Status.Failure,
                _ => Status.Success,
            };
        }
    }
}