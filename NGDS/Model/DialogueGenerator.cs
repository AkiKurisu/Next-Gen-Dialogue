namespace Kurisu.NGDS
{
    /// <summary>
    /// Runtime Dialogue Generator using code
    /// </summary>
    public class DialogueGenerator : IProvideDialogue
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
        DialoguePiece IProvideDialogue.GetNext(string ID)
        {
            var newPiece = dialogueCache.GetPiece(ID);
            return newPiece;
        }
        DialoguePiece IProvideDialogue.GetFirst()
        {
            var piece = dialogueCache.Pieces[0];
            return piece;
        }
        public void AddPiece(DialoguePiece piece)
        {
            dialogueCache.AddPiece(piece);
        }
        public Dialogue GetDialogue()
        {
            return dialogueCache;
        }
    }
}
