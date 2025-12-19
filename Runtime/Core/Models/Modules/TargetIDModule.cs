namespace NextGenDialogue
{
    public class TargetIDModule : IDialogueModule, IModifyNode
    {
        private readonly string _targetID;
        
        public TargetIDModule(string targetID)
        {
            _targetID = targetID;
        }
        
        public void ModifyNode(Node node)
        {
            if (node is Option option)
            {
                option.TargetID = _targetID;
            }
            else
            {
                NextGenDialogueLogger.LogWarning("Target node is not a dialogue option!");
            }
        }
    }
}
