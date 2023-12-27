namespace Kurisu.NGDS
{
    /// <summary>
    /// Interface: dialogue piece proxy
    /// </summary>
    public interface IDialogueProxy
    {
        /// <summary>
        /// Get the next dialogue piece according to the index
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        Piece GetNext(string ID);
        /// <summary>
        /// Get the first dialogue piece
        /// </summary>
        /// <returns></returns>
        Piece GetFirst();
        /// <summary>
        /// Cast the dialogue
        /// </summary>
        /// <returns></returns>
        Dialogue CastDialogue();
    }
}
