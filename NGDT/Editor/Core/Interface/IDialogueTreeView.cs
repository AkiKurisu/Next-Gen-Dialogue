using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
namespace Kurisu.NGDT.Editor
{
    public interface IDialogueTreeView : IVariableSource
    {
        EditorWindow EditorWindow { get; }
        GraphView View { get; }
        IControlGroupBlock GroupBlockController { get; }
        /// <summary>
        /// Copy and paste target node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        IDialogueNode DuplicateNode(IDialogueNode node);
        /// <summary>
        /// Register node and add to graph
        /// </summary>
        /// <param name="node"></param>
        /// <param name="worldRect"></param>
        void AddNode(IDialogueNode node, Rect worldRect);
        ContextualMenuController ContextualMenuController { get; }
        IBlackBoard BlackBoard { get; }
        /// <summary>
        /// On node is selected
        /// </summary>
        /// <value></value>
        Action<IDialogueNode> OnSelectAction { get; }
    }
}
