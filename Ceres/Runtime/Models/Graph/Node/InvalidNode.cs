using Ceres.Annotations;
using UnityEngine;
namespace Ceres.Graph
{
    [NodeGroup("Hidden")]
    [NodeLabel("<color=#FFE000><b>Class Missing!</b></color>")]
    [NodeInfo("The presence of this node indicates that the namespace, class name, or assembly of the node may be changed.")]
    internal sealed class InvalidNode : CeresNode
    {
        [Multiline]
        public string nodeType;
        [Multiline]
        public string serializedData;
    }
}