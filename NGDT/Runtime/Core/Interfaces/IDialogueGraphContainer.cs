using Ceres.Graph;
namespace Kurisu.NGDT
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
