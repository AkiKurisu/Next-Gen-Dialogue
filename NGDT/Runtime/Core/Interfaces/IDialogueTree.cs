using UnityEngine;
using System.Collections.Generic;
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    public interface IDialogueTree
    {
        Object _Object { get; }
        Root Root
        {
            get;
#if UNITY_EDITOR
            set;
#endif
        }
        List<SharedVariable> SharedVariables
        {
            get;
#if UNITY_EDITOR
            set;
#endif
        }
#if UNITY_EDITOR
        /// <summary>
        /// Get external behavior tree, using only in editor
        /// </summary>
        /// <value></value>
        IDialogueTree ExternalBehaviorTree { get; }
        /// <summary>
        /// Get block data from behavior tree graph, using only in editor
        /// </summary>
        /// <value></value>  
        List<GroupBlockData> BlockData { get; set; }
#endif
        IDialogueBuilder Builder { get; }
        IDialogueSystem System { get; set; }
    }
}
