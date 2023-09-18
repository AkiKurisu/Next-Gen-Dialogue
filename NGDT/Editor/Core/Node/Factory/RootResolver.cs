using System;
namespace Kurisu.NGDT.Editor
{
    public class RootResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new RootNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType == typeof(Root);
    }
}
