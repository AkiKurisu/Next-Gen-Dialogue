using System.Collections.Generic;
using Ceres;
using Ceres.Graph;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Interface for dialogue tree graph container
    /// </summary>
    public interface IDialogueContainer : ICeresGraphContainer, IVariableSource
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
        List<NodeGroup> NodeGroups { get; }

        DialogueGraph GetDialogueGraph();
    }
}
