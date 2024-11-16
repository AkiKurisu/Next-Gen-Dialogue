using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UObject = UnityEngine.Object;
namespace Ceres.Graph
{
    public interface ICeresGraphContainer : IVariableSource
    {
        /// <summary>
        /// Container referenced <see cref="UObject"/>
        /// </summary>
        /// <value></value>
        UObject Object { get; }
        /// <summary>
        /// Get graph instance
        /// </summary>
        /// <returns></returns>
        CeresGraph GetGraph();
        /// <summary>
        /// Set graph persistent data
        /// </summary>
        /// <param name="graph"></param>
        void SetGraphData(CeresGraphData graph);
    }
    /* Must set serializable to let managed reference work */
    [Serializable]
    public class CeresGraph: IDisposable
    {
        private BlackBoard _blackBoard;

        /// <summary>
        /// Exposed blackboard for data exchange
        /// </summary>
        public BlackBoard BlackBoard
        {
            get
            {
                return _blackBoard ??= BlackBoard.Create(variables, false);
            }
        }
        
        private readonly HashSet<SharedVariable> internalVariables = new();
        
        private static readonly Dictionary<Type, List<FieldInfo>> fieldInfoLookup = new();
        
        [SerializeReference]
        public List<SharedVariable> variables;

        [SerializeReference] 
        public List<CeresNode> nodes;
        
        public List<NodeGroupBlock> nodeGroupBlocks;

        public CeresGraph()
        {
            
        }
        
        public CeresGraph(CeresGraphData graphData)
        {
            graphData.BuildGraph(this);
        }

        internal List<SharedVariable> GetAllVariablesInternal()
        {
            return variables;
        }
        
        internal void SetVariablesInternal(List<SharedVariable> inVariables)
        {
            variables = inVariables;
        }
        
        internal List<NodeGroupBlock> GetAllNodeGroupBlocksInternal()
        {
            return nodeGroupBlocks;
        }
        internal List<CeresNode> GetAllNodesInternal()
        {
            return nodes;
        }

        /// <summary>
        /// Initialize graph's shared variables
        /// </summary>
        public virtual void InitVariables()
        { 
            InitVariables_Imp(this);
        }

        /// <summary>
        /// Get all nodes owned by this graph
        /// </summary>
        /// <returns></returns>
        public virtual List<CeresNode> GetAllNodes()
        {
            return nodes ?? new();
        }
        
        /// <summary>
        /// Traverse the graph and automatically init all shared variables
        /// </summary>
        /// <param name="graph"></param>
        protected static void InitVariables_Imp(CeresGraph graph)
        {
            HashSet<SharedVariable> internalVariables = graph.internalVariables;
            foreach (var behavior in graph.GetAllNodes())
            {
                var behaviorType = behavior.GetType();
                if (!fieldInfoLookup.TryGetValue(behaviorType, out var fields))
                {
                    fields = behaviorType
                            .GetAllFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                            .Where(x => x.FieldType.IsSubclassOf(typeof(SharedVariable)) || IsIListVariable(x.FieldType))
                            .ToList();
                    fieldInfoLookup.Add(behaviorType, fields);
                }
                foreach (var fieldInfo in fields)
                {
                    var value = fieldInfo.GetValue(behavior);
                    // shared variables should not be null (dsl/builder will cause null variables)
                    if (value == null)
                    {
                        value = Activator.CreateInstance(fieldInfo.FieldType);
                        fieldInfo.SetValue(behavior, value);
                    }
                    if (value is SharedVariable sharedVariable)
                    {
                        sharedVariable.MapTo(graph.BlackBoard);
                        internalVariables.Add(sharedVariable);
                    }
                    else if (value is IList sharedVariableList)
                    {
                        foreach (var variable in sharedVariableList)
                        {
                            var sv = variable as SharedVariable;
                            internalVariables.Add(sv);
                            sv.MapTo(graph.BlackBoard);
                        }
                    }
                }
            }
        }
        private static bool IsIListVariable(Type fieldType)
        {
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type genericArgument = fieldType.GetGenericArguments()[0];
                if (typeof(SharedVariable).IsAssignableFrom(genericArgument))
                {
                    return true;
                }
            }
            else if (fieldType.IsArray)
            {
                Type elementType = fieldType.GetElementType();
                if (typeof(SharedVariable).IsAssignableFrom(elementType))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void Dispose()
        {
            foreach (var variable in variables)
            {
                variable.Unbind();
            }
            foreach (var variable in internalVariables)
            {
                variable.Unbind();
            }
            variables.Clear();
            internalVariables.Clear();
            foreach (var node in GetAllNodes())
            {
                node.Dispose();
            }
        }
    }
    /// <summary>
    /// Expandable graph structure to solve class missing cause children loss
    /// </summary>
    [Serializable]
    public class CeresGraphData
    {
        [Serializable]
        public class Edge
        {
            public int[] children;
        }
        
        [SerializeReference]
        public SharedVariable[] variables;
        
        [SerializeReference]
        public CeresNode[] nodes;
        
        public Edge[] edges;
        
        [SerializeField]
        public CeresNodeData[] nodeData;
        
        [SerializeField]
        public NodeGroupBlock[] nodeGroupBlocks;
        public CeresGraphData() { }
        public CeresGraphData(CeresGraph graph)
        {
            variables = graph.GetAllVariablesInternal().ToArray();
            nodes = graph.GetAllNodesInternal().ToArray();
            edges = new Edge[nodes.Length];
            nodeData = new CeresNodeData[nodes.Length];
            for (int i = 0; i < nodes.Length; ++i)
            {
                var edge = edges[i] = new Edge();
                edge.children = new int[nodes[i].GetChildrenCount()];
                for (int n = 0; n < edge.children.Length; ++n)
                {
                    edge.children[n] = Array.IndexOf(nodes, nodes[i].GetChildAt(n));
                }
                // clear duplicated reference
                nodes[i].ClearChildren();
                nodeData[i] = nodes[i].GetSerializedData();
            }
            nodeGroupBlocks = graph.GetAllNodeGroupBlocksInternal().ToArray();
        }

        /// <summary>
        /// Build graph from data
        /// </summary>
        /// <param name="graph"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void BuildGraph(CeresGraph graph)
        {
            ConnectNodes();
            graph.variables = variables?.ToList() ?? new List<SharedVariable>();
            graph.nodeGroupBlocks = nodeGroupBlocks?.ToList() ?? new List<NodeGroupBlock>();
            graph.nodes = nodes?.ToList() ?? new List<CeresNode>();
        }

        protected void ConnectNodes()
        {
            if (edges == null || edges.Length == 0) return;
            if (nodes == null || nodes.Length == 0) return;
            if (nodes.Length != edges.Length)
            {
                throw new ArgumentException("The length of behaviors and edges must be the same.");
            }
            for (int n = 0; n < nodes.Length; ++n)
            {
                var edge = edges[n];
                var behavior = nodes[n];
                if (nodeData != null && nodeData.Length > n)
                    behavior.nodeData = nodeData[n];
                for (int i = 0; i < edge.children.Length; i++)
                {
                    int childIndex = edge.children[i];
                    if (childIndex >= 0 && childIndex < nodes.Length)
                    {
                        var child = nodes[childIndex];
#if UNITY_EDITOR
                        var config = APIUpdateConfig.GetConfig();
                        if (config)
                        {
                            var pair = config.FindPair(nodeData![childIndex].nodeType);
                            if (pair != null)
                            {
                                Debug.Log($"<color=#3aff48>API Updater</color>: Update node {pair.sourceType.nodeType} to {pair.targetType.nodeType}");
                                nodes[childIndex] = child = (CeresNode)Deserialize(nodeData[childIndex].serializedData, pair.targetType.Type);
                            }
                        }
#endif
                        // use invalid node to replace missing nodes
                        if (child == null)
                        {
                            nodes[childIndex] = GetFallbackNode(nodeData![childIndex], edge);
                        }
                        behavior.AddChild(child);
                    }
                }
            }
        }

        public CeresGraphData Clone()
        {
            // use internal serialization to solve UObject hard reference
            return JsonUtility.FromJson<CeresGraphData>(JsonUtility.ToJson(this));
        }
        
        public T CloneT<T>() where T: CeresGraphData
        {
            // use internal serialization to solve UObject hard reference
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(this));
        }

        /// <summary>
        /// Get fallback node for missing class
        /// </summary>
        /// <param name="fallbackNodeData"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        protected virtual CeresNode GetFallbackNode(CeresNodeData fallbackNodeData, Edge output)
        {
            return new InvalidNode()
            {
                nodeType = fallbackNodeData.nodeType.ToString(),
                serializedData = fallbackNodeData.serializedData
            };
        }
        
        /// <summary>
        /// Serialize json smarter in editor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="indented"></param>
        /// <param name="serializeEditorData"></param>
        /// <param name="verbose"></param>
        /// <returns></returns>
        public static string Serialize(object data, bool indented = false, bool serializeEditorData = false, bool verbose = true)
        {
            if (data == null) return null;
            string json = JsonUtility.ToJson(data, indented);
#if UNITY_EDITOR
            JObject obj = JObject.Parse(json);
            foreach (JProperty prop in obj.Descendants().OfType<JProperty>().ToList())
            {
                // Remove editor only fields manually
                if (!serializeEditorData)
                {
                    if (prop.Name == nameof(nodeData) || prop.Name == nameof(nodeGroupBlocks))
                    {
                        prop.Remove();
                    }
                }
                if (prop.Name == "instanceID")
                {
                    string propertyName = prop.Name;
                    if (prop.Parent?.Parent != null) propertyName = (prop.Parent?.Parent as JProperty).Name;
                    var UObject = EditorUtility.InstanceIDToObject((int)prop.Value);
                    if (UObject == null) continue;
                    string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(UObject));
                    if (string.IsNullOrEmpty(guid))
                    {
                        // TODO: if reference objects inside prefab, may use its relative path
                        if (verbose)
                            Debug.LogWarning($"<color=#fcbe03>Ceres</color>: Can't serialize {propertyName} {UObject} in a Scene.");
                        continue;
                    }
                    //Convert to GUID
                    prop.Value = guid;
                }
            }
            return obj.ToString(indented ? Formatting.Indented : Formatting.None);
#else
            return json;
#endif
        }
        
        /// <summary>
        /// Deserialize <see cref="CeresGraphData"/> from json
        /// </summary>
        /// <param name="serializedData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Deserialize(string serializedData, Type type)
        {
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(serializedData))
            {
                JObject obj = JObject.Parse(serializedData);
                foreach (JProperty prop in obj.Descendants().OfType<JProperty>().ToList())
                {
                    if (prop.Name == "instanceID")
                    {
                        var UObject = AssetDatabase.LoadAssetAtPath<UObject>(AssetDatabase.GUIDToAssetPath((string)prop.Value));
                        if (UObject == null)
                        {
                            prop.Value = 0;
                            continue;
                        }
                        prop.Value = UObject.GetInstanceID();
                    }
                }
                return JsonUtility.FromJson(obj.ToString(Formatting.Indented), type);
            }
#endif
            return JsonUtility.FromJson(serializedData, type);
        }
    }
}
