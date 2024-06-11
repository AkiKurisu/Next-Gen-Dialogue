using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public interface IBlackBoard
    {
        void EditVariable(string variableName);
        void AddVariable(SharedVariable variable, bool fireEvents);
        void RemoveVariable(SharedVariable variable, bool fireEvents);
        Blackboard View { get; }
    }
}