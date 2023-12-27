using System;
namespace Kurisu.NGDT.Editor
{
    public class ConditionalResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new ConditionalNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType.IsSubclassOf(typeof(Conditional));
    }
}
