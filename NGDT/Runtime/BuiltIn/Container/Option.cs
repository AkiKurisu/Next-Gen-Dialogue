namespace Kurisu.NGDT
{
    [AkiInfo("Option is the container of user option")]
    public class Option : Container
    {
        protected override Status OnUpdate()
        {
            Builder.StartWriteNode(NGDS.Option.GetPooled());
            for (var i = 0; i < Children.Count; i++)
            {
                var target = Children[i];
                var childStatus = target.Update();
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

