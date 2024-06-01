using System.Reflection;
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
    }
}