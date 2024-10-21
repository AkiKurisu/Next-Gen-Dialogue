namespace Kurisu.NGDT.Behavior
{
    [NodeInfo("Condition: Compare Bool values, if the conditions are met, return Status.Success, otherwise return Status.Failure")]
    [NodeLabel("Math: BoolComparison")]
    [NodeGroup("Math")]
    public class BoolComparison : Conditional
    {
        public enum Operation
        {
            EqualTo,
            NotEqualTo,
        }
        public SharedBool bool1;
        public SharedBool bool2;
        public Operation operation;
        protected override Status IsUpdatable()
        {
            return operation switch
            {
                Operation.EqualTo => bool1.Value == bool2.Value ? Status.Success : Status.Failure,
                Operation.NotEqualTo => bool1.Value != bool2.Value ? Status.Success : Status.Failure,
                _ => Status.Success,
            };
        }
    }
}