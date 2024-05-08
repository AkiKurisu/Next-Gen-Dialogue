namespace Kurisu.NGDS
{
    /// <summary>
    /// Runtime dialogue builder using code
    /// </summary>
    public class DialogueBuilder : IDialogueLookup
    {
        private Dialogue dialogueCache;
        public DialogueBuilder()
        {
            dialogueCache = Dialogue.GetPooled();
        }
        public void Clear()
        {
            dialogueCache.DisposeRecursively();
            dialogueCache = Dialogue.GetPooled();
        }
        Piece IDialogueLookup.GetNext(string ID)
        {
            var newPiece = dialogueCache.GetPiece(ID);
            return newPiece;
        }
        Piece IDialogueLookup.GetFirst()
        {
            var piece = dialogueCache.Pieces[0];
            return piece;
        }
        public void AddPiece(Piece piece)
        {
            dialogueCache.AddPiece(piece);
        }
        public Dialogue ToDialogue()
        {
            return dialogueCache;
        }
    }
}
