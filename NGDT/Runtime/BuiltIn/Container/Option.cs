using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Option is the container of user option")]
    public class Option : Container
    {
        internal int OptionIndex { get; set; }
        
        protected override Status OnUpdate()
        {
            var node = NGDS.Option.GetPooled();
            node.Index = OptionIndex;
            Builder.StartWriteNode(node);
            foreach (var childNode in Children)
            {
                var childStatus = childNode.Update();
                if (childStatus == Status.Success)
                {
                    continue;
                }
                Builder.DisposeWriteNode();
                return Status.Failure;
            }
            Builder.EndWriteNode();
            return Status.Success;
        }
    }
}

