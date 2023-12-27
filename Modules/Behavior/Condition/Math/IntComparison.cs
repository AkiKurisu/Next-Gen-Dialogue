using UnityEngine;
namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Condition : Compare Int values, if the conditions are met, return Status.Success, otherwise return Status.Failure")]
    [AkiLabel("Math : IntComparison")]
    [AkiGroup("Math")]
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
        [SerializeField]
        private SharedInt int1;
        [SerializeField]
        private SharedInt int2;
        [SerializeField]
        private Operation operation;
        protected override void OnStart()
        {
            InitVariable(int1);
            InitVariable(int2);
        }
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