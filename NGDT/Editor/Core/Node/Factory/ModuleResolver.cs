using System;
namespace Kurisu.NGDT.Editor
{
    public class ModuleResolver : INodeResolver
    {
        public IDialogueNode CreateNodeInstance(Type type)
        {
            if (type.IsSubclassOf(typeof(BehaviorModule)))
            {
                return new BehaviorModuleNode();
            }
            else if (type.IsSubclassOf(typeof(EditorModule)))
            {
                return new EditorModuleNode();
            }
            else
            {
                return new ModuleNode();
            }
        }
        public static bool IsAcceptable(Type behaviorType) => behaviorType.IsSubclassOf(typeof(Module));
    }
}
