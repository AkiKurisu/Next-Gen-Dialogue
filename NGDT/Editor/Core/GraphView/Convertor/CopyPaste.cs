using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class CopyPaste
    {
        private static Graph graph;
        public static void Copy(int instanceID, IEnumerable<GraphElement> elements)
        {
            graph = new Graph()
            {
                instanceID = instanceID,
                edges = elements.OfType<Edge>().ToArray(),
                nodes = elements.OfType<Node>().ToArray()
            };
        }
        public static List<GraphElement> Paste()
        {
            var list = new List<GraphElement>();
            if (graph != null)
            {
                list.AddRange(graph.edges);
                list.AddRange(graph.nodes);
            }
            return list;
        }
        public static bool CanPaste => graph != null && DialogueEditorWindow.ContainsEditorWindow(graph.instanceID);
        public static Vector2 CenterPosition
        {
            get
            {
                if (graph == null) return Vector2.zero;
                var average = Vector2.zero;
                int count = 0;
                foreach (var node in graph.nodes)
                {
                    if (node is ModuleNode or ChildBridge or ParentBridge) continue;
                    count++;
                    average += node.GetPosition().position;
                }
                return average / count;
            }
        }
        private class Graph
        {
            public int instanceID;
            public Edge[] edges;
            public Node[] nodes;
        }
    }
}