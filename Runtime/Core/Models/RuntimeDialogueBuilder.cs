namespace NextGenDialogue
{
    /// <summary>
    /// Runtime dialogue builder using code
    /// </summary>
    public class RuntimeDialogueBuilder : IDialogueContainer
    {
        private Dialogue _dialogueCache = Dialogue.GetPooled();

        public void Clear()
        {
            _dialogueCache.Dispose();
            _dialogueCache = Dialogue.GetPooled();
        }
        
        Piece IDialogueContainer.GetNext(string id)
        {
            var newPiece = _dialogueCache.GetPiece(id);
            return newPiece;
        }
        
        Piece IDialogueContainer.GetFirst()
        {
            var piece = _dialogueCache.Pieces[0];
            return piece;
        }
        
        public void AddPiece(Piece piece)
        {
            _dialogueCache.AddModule(piece);
        }
        
        public Dialogue ToDialogue()
        {
            return _dialogueCache;
        }
    }
}
