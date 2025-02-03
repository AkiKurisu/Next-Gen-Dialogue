using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public class PortHelper
    {
        public static Edge ConnectPorts(Port output, Port input)
        {
            var tempEdge = new Edge
            {
                output = output,
                input = input
            };
            tempEdge.input.Connect(tempEdge);
            tempEdge.output.Connect(tempEdge);
            return tempEdge;
        }
        
        public static IDialogueNodeView FindChildNode(Port port)
        {
            if (port.childCount == 0) return null;
            var child = port.connections.FirstOrDefault()?.input?.node;
            if (child is IDialogueNodeView nodeView)
            {
                return nodeView;
            }
            if (child is ParentBridgeView bridge)
            {
                return bridge.GetFirstAncestorOfType<ContainerNodeView>();
            }
            return null;
        }
        
        public static IDialogueNodeView FindParentNode(Port port)
        {
            if (port.childCount == 0) return null;
            var parent = port.connections.FirstOrDefault()?.output?.node;
            if (parent is IDialogueNodeView nodeView)
            {
                return nodeView;
            }
            if (parent is ChildBridgeView bridge)
            {
                return bridge.GetFirstAncestorOfType<ContainerNodeView>();
            }
            return null;
        }
        
        public static List<Port> GetCompatiblePorts(GraphView graphView, Port startAnchor)
        {
            var compatiblePorts = new List<Port>();
            foreach (var port in graphView.ports.ToList())
            {
                if (startAnchor.node == port.node ||
                    startAnchor.direction == port.direction ||
                    startAnchor.portType != port.portType)
                {
                    continue;
                }
                compatiblePorts.Add(port);
            }
            return compatiblePorts;
        }
    }
}
