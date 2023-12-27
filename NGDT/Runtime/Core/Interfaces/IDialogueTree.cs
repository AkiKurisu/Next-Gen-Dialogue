using UnityEngine;
using System.Collections.Generic;
using Kurisu.NGDS;
namespace Kurisu.NGDT
{
    public interface IDialogueTree : IVariableSource
    {
        Object Object { get; }
        Root Root
        {
            get;
        }
#if UNITY_EDITOR
        /// <summary>
        /// Get block data from behavior tree graph, using only in editor
        /// </summary>
        /// <value></value>  
        List<GroupBlockData> BlockData { get; }
#endif
        IDialogueBuilder Builder { get; }
        IDialogueSystem System { get; set; }
    }
}
