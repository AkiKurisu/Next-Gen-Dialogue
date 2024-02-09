namespace Kurisu.NGDT.Behavior
{
    [AkiInfo("Action: Fixed return value," +
    " returns a fixed value after running, you can put the node at the end of the combination logic to keep the return value.")]
    [AkiLabel("Logic: Fixed Return")]
    [AkiGroup("Logic")]
    public class FixedReturn : Action
    {
        public Status returnStatus;
        protected override Status OnUpdate()
        {
            return returnStatus;
        }
    }
}