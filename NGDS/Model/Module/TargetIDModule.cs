namespace Kurisu.NGDS
{
    public readonly struct TargetIDModule : IDialogueModule, IApplyable
    {
        private readonly string targetID;
        public TargetIDModule(string targetID)
        {
            this.targetID = targetID;
        }
        public void Apply(DialogueNode node)
        {
            if (node is DialogueOption option)
            {
                option.TargetID = targetID;
            }
            else
            {
                NGDSLogger.LogWarning("Target node is not a dialogue option !");
            }
        }
    }
}
