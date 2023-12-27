using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeEditorUtility
    {
        public static void SetRoot(IDialogueTree dialogueTree, Root root)
        {
            dialogueTree.GetType()
            .GetField("root", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(dialogueTree, root);
        }
        public static bool TryGetExternalTree(IDialogueTree dialogueTree, out IDialogueTree externalTree)
        {
            externalTree = dialogueTree.GetType()
            .GetField("externalDialogueTree", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(dialogueTree) as IDialogueTree;
            return externalTree != null;
        }
        internal static Button GetButton(System.Action clickEvent)
        {
            var button = new Button(clickEvent);
            button.style.fontSize = 15;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.color = Color.white;
            return button;
        }
    }
}