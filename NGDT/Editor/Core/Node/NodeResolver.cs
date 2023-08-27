using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class NodeResolver
    {
        private StyleSheet styleSheetCache;
        public IDialogueNode CreateNodeInstance(Type type, IDialogueTreeView treeView)
        {
            IDialogueNode node;
            if (type.IsSubclassOf(typeof(Container)))
            {
                node = GetContainer(type);
            }
            else if (type.IsSubclassOf(typeof(Composite)))
            {
                node = new CompositeNode();
            }
            else if (type.IsSubclassOf(typeof(Conditional)))
            {
                node = new ConditionalNode();
            }
            else if (type.IsSubclassOf(typeof(Decorator)))
            {
                node = new DecoratorNode();
            }
            else if (type.IsSubclassOf(typeof(Module)))
            {
                node = GetModule(type);
            }
            else if (type == typeof(Root))
            {
                node = new RootNode();
            }
            else
            {
                node = new ActionNode();
            }
            node.SetBehavior(type, treeView);
            if (styleSheetCache == null) styleSheetCache = NextGenDialogueSetting.GetNodeStyle();
            (node as Node).styleSheets.Add(styleSheetCache);
            return node;
        }

        private static IDialogueNode GetContainer(Type type)
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
        private static IDialogueNode GetModule(Type type)
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
    }
}