using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
namespace Ceres.Editor
{
    public interface ICeresGraphView : IVariableSource
    {
        EditorWindow EditorWindow { get; }
        CeresBlackboard Blackboard { get; }
    }

    public abstract class CeresGraphView: GraphView, ICeresGraphView
    {
        public List<SharedVariable> SharedVariables { get; } = new();
        
        public EditorWindow EditorWindow { get; set; }
        public CeresBlackboard Blackboard { get; set; }
    }
}