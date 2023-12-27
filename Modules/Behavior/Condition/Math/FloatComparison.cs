using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Condition : Compare Float values, if the condition is met, return Status.Success, otherwise return Status.Failure")]
    [AkiLabel("Math : FloatComparison")]
    [AkiGroup("Math")]
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
        [SerializeField]
        private SharedFloat float1;
        [SerializeField]
        private SharedFloat float2;
        [SerializeField]
        private Operation operation;
        protected override void OnStart()
        {
            InitVariable(float1);
            InitVariable(float2);
        }
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