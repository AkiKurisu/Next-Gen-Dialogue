using System.Reflection;
using UnityEngine.Assertions;
namespace Kurisu.NGDT.Editor
{
    public static class DialogueContainerUtility
    {
        public static void SetRoot(IDialogueGraphContainer dialogueGraphTree, Root root)
        {
            var field = dialogueGraphTree.GetType().GetField("root", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsTrue(field != null);
            field.SetValue(dialogueGraphTree, root);
        }
        public static bool TryGetExternalTree(IDialogueGraphContainer dialogueGraphTree, out IDialogueGraphContainer externalTree)
        {
            externalTree = dialogueGraphTree.GetType()
            .GetField("externalDialogueAsset", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.GetValue(dialogueGraphTree) as IDialogueGraphContainer;
            return externalTree != null;
        }
    }
}