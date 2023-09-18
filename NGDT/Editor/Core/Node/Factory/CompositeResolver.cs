using System;
namespace Kurisu.NGDT.Editor
{
    public class CompositeResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            return new CompositeNode();
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType.IsSubclassOf(typeof(Composite));
    }
}
