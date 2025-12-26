using System;
using Ceres.Annotations;

namespace NextGenDialogue.Graph
{
    [Obsolete("Sequence is no longer used, use Flow instead.")]
    [NodeInfo("Composite: Sequence, traversing the child nodes in turn, if it returns Success" +
    ", continue to update the next one, otherwise it returns Failure")]
    public class Sequence : CompositeNode
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