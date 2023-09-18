using System;
namespace Kurisu.NGDT.Editor
{
    public class DecoratorResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new DecoratorNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType.IsSubclassOf(typeof(Decorator));
    }
}
