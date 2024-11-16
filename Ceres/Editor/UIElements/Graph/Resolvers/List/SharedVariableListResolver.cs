using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    [ResolveChild]
    public class SharedVariableListResolver<T> : ListResolver<T> where T : SharedVariable, new()
    {
        public SharedVariableListResolver(FieldInfo fieldInfo, IFieldResolver resolver) : base(fieldInfo, resolver)
        {
        }
        protected override ListField<T> CreateEditorField(FieldInfo fieldInfo)
        {
            return new SharedVariableListField<T>(fieldInfo.Name, () => childResolver.CreateField(), () => new T());
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _)
        {
            if (infoType.IsGenericType && infoType.GetGenericTypeDefinition() == typeof(List<>) && infoType.GenericTypeArguments[0].IsSubclassOf(typeof(SharedVariable))) return true;
            if (infoType.IsArray && infoType.GetElementType().IsSubclassOf(typeof(SharedVariable))) return true;
            return false;
        }
    }
    public class SharedVariableListField<T> : ListField<T>, IBindableField where T : SharedVariable
    {
        private CeresGraphView graphView;
        private Action<CeresGraphView> onTreeViewInitEvent;
        public SharedVariableListField(string label, Func<VisualElement> elementCreator, Func<object> valueCreator) : base(label, elementCreator, valueCreator)
        {

        }
        public void BindGraph(CeresGraphView graphView)
        {
            this.graphView = graphView;
            onTreeViewInitEvent?.Invoke(graphView);
        }
        protected override ListView CreateListView()
        {
            void BindItem(VisualElement e, int i)
            {
                ((BaseField<T>)e).value = value[i];
                ((BaseField<T>)e).RegisterValueChangedCallback((x) => value[i] = x.newValue);
            }
            VisualElement MakeItem()
            {
                var field = elementCreator.Invoke();
                ((BaseField<T>)field).label = string.Empty;
                if (graphView != null) (field as IBindableField)?.BindGraph(graphView);
                onTreeViewInitEvent += (view) => { (field as IBindableField)?.BindGraph(view); };
                return field;
            }
            var view = new ListView(value, 20, MakeItem, BindItem);
            return view;
        }

    }
}