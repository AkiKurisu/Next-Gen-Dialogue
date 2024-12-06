using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public class CopyPaste
    {
        private static GraphStructure _graphStructure;
        
        /// <summary>
        /// Copy elements to global without serialization
        /// </summary>
        /// <param name="instanceID">EditorWindow InstanceID</param>
        /// <param name="elements"></param>
        public static void Copy(int instanceID, GraphElement[] elements)
        {
            _graphStructure = new GraphStructure()
            {
                InstanceID = instanceID,
                Edges = elements.OfType<Edge>().ToArray(),
                Nodes = elements.OfType<Node>().ToArray()
            };
        }
        
        public static List<UnityEditor.Experimental.GraphView.GraphElement> Paste()
        {
            var list = new List<UnityEditor.Experimental.GraphView.GraphElement>();
            if (_graphStructure == null) return list;
            
            list.AddRange(_graphStructure.Edges);
            list.AddRange(_graphStructure.Nodes);
            return list;
        }
        
        public static bool CanPaste => _graphStructure?.IsValid() ?? false;

        public static Vector2 CenterPosition
        {
            get
            {
                if (_graphStructure == null) return Vector2.zero;
                var average = Vector2.zero;
                int count = 0;
                foreach (var node in _graphStructure.Nodes)
                {
                    if (node is ChildBridge or ParentBridge) continue;
                    count++;
                    if (node is ModuleNode moduleNode)
                    {
                        average += moduleNode.GetWorldPosition().position;
                    }
                    else
                    {
                        average += node.GetPosition().position;
                    }
                }
                return average / count;
            }
        }
        
        private class GraphStructure
        {
            public int InstanceID;
            
            public Edge[] Edges;
            
            public Node[] Nodes;

            public bool IsValid()
            {
               return DialogueEditorWindow.EditorWindowRegistry.FindWindow(InstanceID);
            }
        }
    }
}