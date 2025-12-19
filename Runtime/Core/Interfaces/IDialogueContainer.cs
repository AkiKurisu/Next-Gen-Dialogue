namespace NextGenDialogue
{
    public interface IDialogueContainer
    {
        /// <summary>
        /// Get the next <see cref="Piece"/> by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Piece GetNext(string id);
        
        /// <summary>
        /// Get the first <see cref="Piece"/>
        /// </summary>
        /// <returns></returns>
        Piece GetFirst();
        
        /// <summary>
        /// Cast the <see cref="Dialogue"/>
        /// </summary>
        /// <returns></returns>
        Dialogue ToDialogue();
    }
}
