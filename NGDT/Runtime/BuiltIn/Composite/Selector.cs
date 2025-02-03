using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Composite : Select, traverse the child nodes in turn," +
    " if it returns Failure, continue to update the next one, otherwise return Success")]
    public class Selector : CompositeNode
    {
        protected override Status OnUpdate()
        {
            return UpdateWhileFailure(0);
        }

        private Status UpdateWhileFailure(int start)
        {
            for (var i = start; i < Children.Count; i++)
            {
                var target = Children[i];
                var childStatus = target.Update();
                if (childStatus == Status.Failure)
                {
                    continue;
                }
                return childStatus;
            }

            return Status.Failure;
        }
    }
}