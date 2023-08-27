namespace Kurisu.NGDS
{
    /// <summary>
    /// Interface: Provide dialogue piece
    /// </summary>
    public interface IProvideDialogue
    {
        /// <summary>
        /// Get the next dialogue piece according to the index
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        DialoguePiece GetNext(string ID);
        /// <summary>
        /// Get the first dialogue piece
        /// </summary>
        /// <returns></returns>
        DialoguePiece GetFirst();
        /// <summary>
        /// Get the dialogue
        /// </summary>
        /// <returns></returns>
        Dialogue GetDialogue();
    }
}
