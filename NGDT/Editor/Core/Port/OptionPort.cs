using System;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public class OptionPort : Port
    {
        protected OptionPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }
    }
}
