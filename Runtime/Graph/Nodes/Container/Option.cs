using Ceres.Annotations;

namespace NextGenDialogue.Graph
{
    [NodeInfo("Define the dialogue option.")]
    public class Option : ContainerNode
    {
        internal int OptionIndex { get; set; }
        
        protected override Status OnUpdate()
        {
            var node = NextGenDialogue.Option.GetPooled();
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

