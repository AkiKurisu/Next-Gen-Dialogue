using Ceres;
using Ceres.Annotations;

namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Condition: Compare Float values, if the condition is met, return Status.Success, otherwise return Status.Failure")]
    [CeresLabel("Math: FloatComparison")]
    [NodeGroup("Math")]
    public class FloatComparison : Conditional
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
        public SharedFloat float1;
        public SharedFloat float2;
        public Operation operation;
        protected override Status IsUpdatable()
        {
            return operation switch
            {
                Operation.LessThan => float1.Value < float2.Value ? Status.Success : Status.Failure,
                Operation.LessThanOrEqualTo => float1.Value <= float2.Value ? Status.Success : Status.Failure,
                Operation.EqualTo => float1.Value == float2.Value ? Status.Success : Status.Failure,
                Operation.NotEqualTo => float1.Value != float2.Value ? Status.Success : Status.Failure,
                Operation.GreaterThanOrEqualTo => float1.Value >= float2.Value ? Status.Success : Status.Failure,
                Operation.GreaterThan => float1.Value > float2.Value ? Status.Success : Status.Failure,
                _ => Status.Success,
            };
        }
    }
}