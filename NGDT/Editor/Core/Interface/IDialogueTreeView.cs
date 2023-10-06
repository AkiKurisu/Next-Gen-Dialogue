using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public interface IDialogueTreeView
    {
        GraphView View { get; }
        void SelectGroup(IDialogueNode node);
        void UnSelectGroup();
        IDialogueNode DuplicateNode(IDialogueNode node);
        event Action<SharedVariable> OnPropertyNameChange;
        List<SharedVariable> ExposedProperties { get; }
        IBlackBoard BlackBoard { get; }
        Action<IDialogueNode> OnSelectAction { get; }
        void BakeDialogue();
    }
}
