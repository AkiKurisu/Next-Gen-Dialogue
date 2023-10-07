using System;
namespace Kurisu.NGDT.Editor
{
    public interface IBlackBoard
    {
        event Action<SharedVariable> OnPropertyNameChange;
        void EditProperty(string variableName);
        void AddExposedProperty(SharedVariable variable, bool canDuplicate);
    }
}