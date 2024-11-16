using System.Reflection;
namespace Kurisu.NGDT.Editor
{
    public class DialogueTreeEditorUtility
    {
        public static void SetRoot(IDialogueContainer dialogueTree, Root root)
        {
            dialogueTree.GetType()
            .GetField("root", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(dialogueTree, root);
        }
        public static bool TryGetExternalTree(IDialogueContainer dialogueTree, out IDialogueContainer externalTree)
        {
            externalTree = dialogueTree.GetType()
            .GetField("externalDialogueTree", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(dialogueTree) as IDialogueContainer;
            return externalTree != null;
        }
    }
}