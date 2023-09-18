using System;
namespace Kurisu.NGDT.Editor
{
    public class ContainerResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            if (type == typeof(Dialogue))
            {
                return new DialogueContainer();
            }
            else if (type == typeof(Piece))
            {
                return new PieceContainer();
            }
            else if (type == typeof(Option))
            {
                return new OptionContainer();
            }
            throw new Exception("Container type is not valid !");
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType.IsSubclassOf(typeof(Container));
    }
}
