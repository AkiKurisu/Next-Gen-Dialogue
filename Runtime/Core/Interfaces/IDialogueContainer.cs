namespace NextGenDialogue
{
    public interface IDialogueContainer
    {
        /// <summary>
        /// Get the next dialogue piece according to the index
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Piece GetNext(string id);
        
        /// <summary>
        /// Get the first dialogue piece
        /// </summary>
        /// <returns></returns>
        Piece GetFirst();
        
        /// <summary>
        /// Cast the dialogue
        /// </summary>
        /// <returns></returns>
        Dialogue ToDialogue();
    }
}
