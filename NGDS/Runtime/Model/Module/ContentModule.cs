namespace Kurisu.NGDS
{
    public readonly struct ContentModule : IDialogueModule, IApplyable
    {
        private readonly string content;
        public ContentModule(string content)
        {
            this.content = content;
        }
        public void Apply(Node node)
        {
            if (node is IContent content)
            {
                content.Content = this.content;
            }
            else
            {
                NGDSLogger.LogWarning("Target node don't have content to modify !");
            }
        }
    }
}
