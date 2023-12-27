using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public enum ContextualMenuType
    {
        Graph, Node
    }
    public interface IContextualMenuBuilder
    {
        bool CanBuild(Type constraintBehaviorType);
        ContextualMenuType MenuType { get; }
        void BuildContextualMenu(ContextualMenuPopulateEvent evt);
    }
    public class ContextualMenuController
    {
        private readonly Dictionary<Type, IContextualMenuBuilder> builderMap = new();
        public void Register(Type type, IContextualMenuBuilder builder)
        {
            builderMap[type] = builder;
        }
        public void Register<T>(IContextualMenuBuilder builder)
        {
            Register(typeof(T), builder);
        }
        public void UnRegister<T>()
        {
            UnRegister(typeof(T));
        }
        public void UnRegister(Type nodeType)
        {
            if (builderMap.ContainsKey(nodeType))
                builderMap.Remove(nodeType);
        }
        public void BuildContextualMenu(ContextualMenuType menuType, ContextualMenuPopulateEvent evt, Type constraintType)
        {
            foreach (var builder in builderMap.Values)
            {
                if (!builder.CanBuild(constraintType))
                    continue;
                if (builder.MenuType == menuType)
                    builder.BuildContextualMenu(evt);
            }
        }
    }
    public class ContextualMenuBuilder : IContextualMenuBuilder
    {
        public ContextualMenuType MenuType { get; }
        public Func<Type, bool> CanBuildFunc { get; }
        public Action<ContextualMenuPopulateEvent> OnBuildContextualMenu { get; }
        public ContextualMenuBuilder(ContextualMenuType contextualMenuType, Func<Type, bool> CanBuildFunc, Action<ContextualMenuPopulateEvent> OnBuildContextualMenu)
        {
            MenuType = contextualMenuType;
            this.CanBuildFunc = CanBuildFunc;
            this.OnBuildContextualMenu = OnBuildContextualMenu;
        }
        public void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            OnBuildContextualMenu(evt);
        }
        public bool CanBuild(Type constraintBehaviorType)
        {
            return CanBuildFunc(constraintBehaviorType);
        }
    }
}