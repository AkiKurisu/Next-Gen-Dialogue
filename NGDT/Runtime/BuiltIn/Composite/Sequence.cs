namespace Kurisu.NGDT
{
    [AkiInfo("Composite : Sequence, traversing the child nodes in turn, if it returns Success" +
    ", continue to update the next one, otherwise it returns Failure")]
    public class Sequence : Composite
    {

        protected override Status OnUpdate()
        {
            return UpdateWhileSuccess(0);

        }

        private Status UpdateWhileSuccess(int start)
        {
            for (var i = start; i < Children.Count; i++)
            {
                var target = Children[i];
                var childStatus = target.Update();
                if (childStatus == Status.Success)
                {
                    continue;
                }
                return childStatus;
            }
            return Status.Success;
        }
    }
}