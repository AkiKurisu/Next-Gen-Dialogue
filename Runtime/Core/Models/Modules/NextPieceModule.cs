namespace NextGenDialogue
{
    public class NextPieceModule : IDialogueModule
    {
        public string NextID { get; }
        
        public NextPieceModule(string nextID)
        {
            NextID = nextID;
        }
    }
}
