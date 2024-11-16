using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Node;
using UnityEngine;
using UObject = UnityEngine.Object;
namespace Ceres
{
    public interface ICeresGraphContainer : IVariableSource
    {
        /// <summary>
        /// Container referenced <see cref="UnityEngine.Object"/>
        /// </summary>
        /// <value></value>
        UObject Object { get; }
        /// <summary>
        /// Get graph instance
        /// </summary>
        /// <returns></returns>
        CeresGraphInstance GetGraphInstance();
        /// <summary>
        /// Set graph persistent data
        /// </summary>
        /// <param name="graph"></param>
        void SetGraph(CeresGraph graph);
    }

    public class CeresGraph: IDisposable
    {
        // Exposed blackboard for data exchange
        public BlackBoard BlackBoard { get; private set; }
        
        private readonly HashSet<SharedVariable> internalVariables = new();
        
        private static readonly Dictionary<Type, List<FieldInfo>> fieldInfoLookup = new();
        
        [SerializeReference]
        protected List<SharedVariable> variables;

        [SerializeReference] 
        protected List<CeresNode> nodes;
        
        /// <summary>
        /// Initialize graph's shared variables
        /// </summary>
        public void InitVariables()
        {
            BlackBoard ??= BlackBoard.Create(variables, false);
            InitVariables_Imp(this);
        }

        /// <summary>
        /// Get all nodes owned by this graph
        /// </summary>
        /// <returns></returns>
        public virtual List<CeresNode> GetAllNodes()
        {
            return nodes ??= new();
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

        public void Dispose()
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
    
    public class CeresGraphInstance
    {
        
    }
}
