using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
namespace Kurisu.NGDT.Editor
{
    public interface IDialogueTreeView
    {
        public EditorWindow EditorWindow { get; }
        GraphView View { get; }
        IControlGroupBlock GroupBlockController { get; }
        IDialogueNode DuplicateNode(IDialogueNode node);
        List<SharedVariable> ExposedProperties { get; }
        IBlackBoard BlackBoard { get; }
        Action<IDialogueNode> OnSelectAction { get; }
        void BakeDialogue();
    }
}
