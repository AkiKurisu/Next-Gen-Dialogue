using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Decorator: If the child node returns Success, it is reversed to Failure," +
    " if it is Failure, it is reversed to Success.")]
    [CeresLabel("Invertor")]
    public class Invertor : Decorator
    {
        protected override Status OnDecorate(Status childStatus)
        {
            if (childStatus == Status.Success)
                return Status.Failure;
            return Status.Success;
        }
    }
}