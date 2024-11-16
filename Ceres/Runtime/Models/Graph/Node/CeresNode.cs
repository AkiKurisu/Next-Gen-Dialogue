using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Ceres.Annotations;
using UnityEngine;
using UnityEngine.Pool;
namespace Ceres.Graph
{
    /// <summary>
    /// Base class for ceres graph node
    /// </summary>
    [Serializable]
    public class CeresNode: IEnumerable<CeresNode>, IDisposable
    {
        [HideInEditorWindow, NonSerialized]
        public CeresNodeData nodeData = new();

        public string GUID 
        { 
            get => nodeData.guid; 
            set => nodeData.guid = value; 
        }
        
        /// <summary>
        /// Release on node destroy
        /// </summary>
        public virtual void Dispose()
        {

        }

        /// <summary>
        /// Get child not at index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual CeresNode GetChildAt(int index)
        {
            return null;
        }
        
        /// <summary>
        /// Add new child node
        /// </summary>
        /// <param name="node"></param>
        public virtual void AddChild(CeresNode node)
        {

        }
        
        /// <summary>
        /// Get child node count
        /// </summary>
        /// <returns></returns>
        public virtual int GetChildrenCount() => 0;
        
        /// <summary>
        /// Clear all child nodes
        /// </summary>
        public virtual void ClearChildren() { }
        
        /// <summary>
        ///  Get all child nodes
        /// </summary>
        /// <returns></returns>
        public virtual CeresNode[] GetChildren()
        {
            return Array.Empty<CeresNode>();
        }
        
        /// <summary>
        /// Set child nodes
        /// </summary>
        /// <param name="children"></param>
        public virtual void SetChildren(CeresNode[] children) { }
        
        /// <summary>
        /// Get serialized data of this node
        /// </summary>
        /// <returns></returns>
        public virtual CeresNodeData GetSerializedData()
        {
            /* Allows polymorphic serialization */
            var data = nodeData.Clone();
            data.Serialize(this);
            return data;
        }
        
        protected struct Enumerator : IEnumerator<CeresNode>
        {
            private readonly Stack<CeresNode> stack;
            private static readonly ObjectPool<Stack<CeresNode>> pool = new(() => new(), null, s => s.Clear());
            private CeresNode currentNode;
            public Enumerator(CeresNode root)
            {
                stack = pool.Get();
                currentNode = null;
                if (root != null)
                {
                    stack.Push(root);
                }
            }

            public readonly CeresNode Current
            {
                get
                {
                    if (currentNode == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return currentNode;
                }
            }

            readonly object IEnumerator.Current => Current;

            public void Dispose()
            {
                pool.Release(stack);
                currentNode = null;
            }
            public bool MoveNext()
            {
                if (stack.Count == 0)
                {
                    return false;
                }

                currentNode = stack.Pop();
                int childrenCount = currentNode.GetChildrenCount();
                for (int i = childrenCount - 1; i >= 0; i--)
                {
                    stack.Push(currentNode.GetChildAt(i));
                }
                return true;
            }
            public void Reset()
            {
                stack.Clear();
                if (currentNode != null)
                {
                    stack.Push(currentNode);
                }
                currentNode = null;
            }
        }

        public IEnumerator<CeresNode> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
    }
    /// <summary>
    /// Class store the node editor data
    /// </summary>
    [Serializable]
    public class CeresNodeData
    {
        /// <summary>
        /// Serialized node type in ManagedReference format
        /// </summary>
        [Serializable]
        public struct NodeType
        {
            public string _class;

            public string _ns;
            
            public string _asm;
            
            public NodeType(string _class, string _ns, string _asm)
            {
                this._class = _class;
                this._ns = _ns;
                this._asm = _asm;
            }
            
            public NodeType(Type type)
            {
                _class = type.Name;
                _ns = type.Namespace;
                _asm = type.Assembly.GetName().Name;
            }
            
            public readonly Type ToType()
            {
                return Type.GetType(Assembly.CreateQualifiedName(_asm, $"{_ns}.{_class}"));
            }
            
            public readonly override string ToString()
            {
                return $"class:{_class} ns: {_ns} asm:{_asm}";
            }
        }
        public Rect graphPosition = new(400, 300, 100, 100);
        
        public string description;
        
        public string guid;
        
        /// <summary>
        /// Node type that helps to locate and recover node when missing class
        /// </summary>
        public NodeType nodeType;
        
        public string serializedData;
        
        /// <summary>
        /// Serialize node data
        /// </summary>
        /// <param name="node"></param>
        public virtual void Serialize(CeresNode node)
        {
            nodeType = new NodeType(node.GetType());
            serializedData = CeresGraphData.Serialize(node);
            /* Override to customize serialization like ISerializationCallbackReceiver */
        }
        
        public virtual CeresNodeData Clone()
        {
            return new CeresNodeData
            {
                graphPosition = graphPosition,
                description = description,
                guid = guid,
                nodeType = nodeType,
                serializedData = serializedData
            };
        }
    }
}