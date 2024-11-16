using System.Collections.Generic;
using Ceres.Graph;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Interface for dialogue tree graph container
    /// </summary>
    public interface IDialogueTree : ICeresGraphContainer
    {
        Root Root
        {
            get;
        }
        // TODO: Remove
        /// <summary>
        /// Get block data from behavior tree graph
        /// </summary>
        /// <value></value>  
        List<NodeGroupBlock> BlockData { get; }
    }
}
