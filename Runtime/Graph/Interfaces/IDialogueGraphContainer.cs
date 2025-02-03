using Ceres.Graph;
namespace NextGenDialogue.Graph
{
    /// <summary>
    /// Interface for dialogue graph container
    /// </summary>
    public interface IDialogueGraphContainer : ICeresGraphContainer
    {
        /// <summary>
        /// Get dialogue graph instance
        /// </summary>
        /// <returns></returns>
        DialogueGraph GetDialogueGraph();
    }
}
