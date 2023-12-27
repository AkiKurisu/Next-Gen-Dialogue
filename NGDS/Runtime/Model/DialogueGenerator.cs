namespace Kurisu.NGDS
{
    /// <summary>
    /// Runtime Dialogue Generator using code
    /// </summary>
    public class DialogueGenerator : IDialogueProxy
    {
        private Dialogue dialogueCache;
        public DialogueGenerator()
        {
            dialogueCache = Dialogue.CreateDialogue();
        }
        public void Clear()
        {
            dialogueCache.NodePushPool();
            dialogueCache = Dialogue.CreateDialogue();
        }
        Piece IDialogueProxy.GetNext(string ID)
        {
            var newPiece = dialogueCache.GetPiece(ID);
            return newPiece;
        }
        Piece IDialogueProxy.GetFirst()
        {
            var piece = dialogueCache.Pieces[0];
            return piece;
        }
        public void AddPiece(Piece piece)
        {
            dialogueCache.AddPiece(piece);
        }
        public Dialogue CastDialogue()
        {
            return dialogueCache;
        }
    }
}
