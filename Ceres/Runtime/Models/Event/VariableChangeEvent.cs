using UnityEngine.UIElements;
namespace Ceres
{
    public enum VariableChangeType
    {
        Create,
        ValueChange,
        NameChange,
        Delete
    }
    public class VariableChangeEvent : EventBase<VariableChangeEvent>
    {
        public SharedVariable Variable { get; protected set; }
        public VariableChangeType ChangeType { get; protected set; }
        public static VariableChangeEvent GetPooled(SharedVariable notifyVariable, VariableChangeType changeType)
        {
            VariableChangeEvent changeEvent = GetPooled();
            changeEvent.Variable = notifyVariable;
            changeEvent.ChangeType = changeType;
            return changeEvent;
        }
    }
}
