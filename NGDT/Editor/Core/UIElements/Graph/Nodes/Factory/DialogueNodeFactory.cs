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
    
    public class DialogueNodeFactory
    {
        private static DialogueNodeFactory _instance;

        public static DialogueNodeFactory Get()
        {
            return _instance ??= new DialogueNodeFactory();
        }
        
        private StyleSheet _styleSheetCache;
        
        private readonly List<Type> _resolverTypes;
        
        public DialogueNodeFactory()
        {
            _instance = this;
            _resolverTypes = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .Select(x => x.GetTypes())
                                        .SelectMany(x => x)
                                        .Where(IsValidType)
                                        .ToList();
            _resolverTypes.Sort((a, b) =>
            {
                var aCustom = a.GetCustomAttribute<CustomNodeViewAttribute>(false);
                var bCustom = b.GetCustomAttribute<CustomNodeViewAttribute>(false);
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
            if (type.GetCustomAttribute<CustomNodeViewAttribute>() != null) return true;
            if (type.GetMethod("IsAcceptable") == null) return false;
            if (type.GetInterfaces().All(t => t != typeof(INodeResolver))) return false;
            return true;
        }
        
        public IDialogueNode Create(Type behaviorType, DialogueGraphView graphView)
        {
            IDialogueNode node = null;
            bool find = false;
            foreach (var resolverType in _resolverTypes)
            {
                var attribute = resolverType.GetCustomAttribute<CustomNodeViewAttribute>();
                if (attribute != null)
                {
                    if (TryAcceptNodeEditor(attribute, behaviorType))
                    {
                        node = (IDialogueNode)Activator.CreateInstance(resolverType);
                        find = true;
                        break;
                    }
                    continue;
                }
                if (!IsAcceptable(resolverType, behaviorType)) continue;
                node = ((INodeResolver)Activator.CreateInstance(resolverType)).CreateNodeInstance(behaviorType);
                find = true;
                break;
            }
            if (!find) node = new ActionNode();
            node.SetNodeType(behaviorType, graphView);
            if (_styleSheetCache == null) _styleSheetCache = NextGenDialogueSettings.GetNodeStyle();
            ((Node)node).styleSheets.Add(_styleSheetCache);
            return node;
        }
        
        private bool TryAcceptNodeEditor(CustomNodeViewAttribute attribute, Type behaviorType)
        {
            if (attribute.NodeType == behaviorType) return true;
            if (attribute.CanInherit && behaviorType.IsSubclassOf(attribute.NodeType)) return true;
            return false;
        }
        
        private static bool IsAcceptable(Type type, Type behaviorType)
        {
            return (bool)type.InvokeMember("IsAcceptable", BindingFlags.InvokeMethod, null, null, new object[] { behaviorType });
        }
    }
}