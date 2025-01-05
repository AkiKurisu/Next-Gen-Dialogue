using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Interface for dialogue tree graph container
    /// </summary>
    public interface IDialogueGraphContainer : ICeresGraphContainer, IVariableSource
    {
        // TODO: Remove
        Root Root
        {
            get;
        }

        /// <summary>
        /// Get block data from behavior tree graph
        /// </summary>
        /// <value></value>  
        List<NodeGroup> NodeGroups { get; }
    }
}
