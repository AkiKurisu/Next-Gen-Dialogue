using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public interface IBlackBoard
    {
        void EditProperty(string variableName);
        void AddSharedVariable(SharedVariable variable);
        Blackboard View { get; }
    }
}