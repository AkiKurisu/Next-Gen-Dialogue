using System.Reflection;
using UnityEngine.Assertions;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueContainerUtility
    {
        public static void SetRoot(IDialogueContainer dialogueTree, Root root)
        {
            var field = dialogueTree.GetType().GetField("root", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsTrue(field != null);
            field.SetValue(dialogueTree, root);
        }
        public static bool TryGetExternalTree(IDialogueContainer dialogueTree, out IDialogueContainer externalTree)
        {
            externalTree = dialogueTree.GetType()
            .GetField("externalDialogueAsset", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(dialogueTree) as IDialogueContainer;
            return externalTree != null;
        }
    }
}