namespace NextGenDialogue
{
    public class ContentModule : IDialogueModule, IModifyNode
    {
        private readonly string _content;
        
        public ContentModule(string content)
        {
            _content = content;
        }
        
        public void ModifyNode(Node node)
        {
            if (node is IContentModule contentModule)
            {
                contentModule.AddContent(_content);
            }
            else
            {
                NextGenDialogueLogger.LogWarning("Target node don't have content to modify!");
            }
        }
    }
}
