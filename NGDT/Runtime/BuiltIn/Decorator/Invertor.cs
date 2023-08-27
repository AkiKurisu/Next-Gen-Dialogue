namespace Kurisu.NGDT
{
    [AkiInfo("Decorator : If the child node returns Success, it is reversed to Failure," +
    " if it is Failure, it is reversed to Success.")]
    [AkiLabel("Invertor")]
    public class Invertor : Decorator
    {

        protected override Status OnDecorate(Status childStatus)
        {
            if (childStatus == Status.Success)
                return Status.Failure;
            else
                return Status.Success;
        }
    }
}