using System;
using UnityEditor.Experimental.GraphView;
namespace NextGenDialogue.Graph.Editor
{
    public class PiecePort : Port
    {
        protected PiecePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }
    }
}
