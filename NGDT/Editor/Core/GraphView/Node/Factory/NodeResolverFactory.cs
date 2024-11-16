using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public interface INodeResolver
    {
        IDialogueNode CreateNodeInstance(Type type);
    }
    public class NodeResolverFactory
    {
        private static NodeResolverFactory instance;
        public static NodeResolverFactory Instance => instance ?? new NodeResolverFactory();
        private StyleSheet styleSheetCache;
        private readonly List<Type> _ResolverTypes = new();
        public NodeResolverFactory()
        {
            instance = this;
            _ResolverTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .Select(x => x.GetTypes())
            .SelectMany(x => x)
            .Where(x => IsValidType(x))
            .ToList();
            _ResolverTypes.Sort((a, b) =>
            {
                var aCustom = a.GetCustomAttribute<CustomNodeEditorAttribute>(false);
                var bCustom = b.GetCustomAttribute<CustomNodeEditorAttribute>(false);
                var aOrdered = a.GetCustomAttribute<OrderedAttribute>(false);
                var bOrdered = b.GetCustomAttribute<OrderedAttribute>(false);
                if (aCustom == null && bCustom != null) return 1;
                if (aCustom != null && bCustom == null) return -1;
                if (aOrdered == null && bOrdered == null) return 0;
                if (aOrdered != null && bOrdered != null) return aOrdered.Order - bOrdered.Order;
                if (aOrdered != null) return -1;
                return 1;
            });
        }
        private static bool IsValidType(Type type)
        {
            if (type.IsAbstract) return false;
            if (type.GetCustomAttribute<CustomNodeEditorAttribute>() != null) return true;
            if (type.GetMethod("IsAcceptable") == null) return false;
            if (!type.GetInterfaces().Any(t => t == typeof(INodeResolver))) return false;
            return true;
        }
        public IDialogueNode Create(Type behaviorType, DialogueTreeView treeView)
        {
            IDialogueNode node = null;
            bool find = false;
            foreach (var _resolverType in _ResolverTypes)
            {
                var attribute = _resolverType.GetCustomAttribute<CustomNodeEditorAttribute>();
                if (attribute != null)
                {
                    if (TryAcceptNodeEditor(attribute, behaviorType))
                    {
                        node = (IDialogueNode)Activator.CreateInstance(_resolverType);
                        find = true;
                        break;
                    }
                    continue;
                }
                if (!IsAcceptable(_resolverType, behaviorType)) continue;
                node = (Activator.CreateInstance(_resolverType) as INodeResolver).CreateNodeInstance(behaviorType);
                find = true;
                break;
            }
            if (!find) node = new ActionNode();
            node.SetBehavior(behaviorType, treeView);
            if (styleSheetCache == null) styleSheetCache = NextGenDialogueSetting.GetNodeStyle();
            (node as Node).styleSheets.Add(styleSheetCache);
            return node;
        }
        private bool TryAcceptNodeEditor(CustomNodeEditorAttribute attribute, Type behaviorType)
        {
            if (attribute.InspectedType == behaviorType) return true;
            if (attribute.EditorForChildClasses && behaviorType.IsSubclassOf(attribute.InspectedType)) return true;
            return false;
        }
        private static bool IsAcceptable(Type type, Type behaviorType)
        {
            return (bool)type.InvokeMember("IsAcceptable", BindingFlags.InvokeMethod, null, null, new object[] { behaviorType });
        }
    }
}