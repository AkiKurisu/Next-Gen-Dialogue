using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [ResolveChild]
    public class SharedVariableListResolver<T> : FieldResolver<SharedVariableListField<T>, List<T>> where T : SharedVariable, new()
    {
        private readonly IFieldResolver childResolver;
        public SharedVariableListResolver(FieldInfo fieldInfo, IFieldResolver resolver) : base(fieldInfo)
        {
            childResolver = resolver;
        }
        SharedVariableListField<T> editorField;
        protected override void SetTree(IDialogueTreeView ownerTreeView)
        {
            editorField.Init(ownerTreeView);
        }
        protected override SharedVariableListField<T> CreateEditorField(FieldInfo fieldInfo)
        {
            editorField = new SharedVariableListField<T>(fieldInfo.Name, null, () => childResolver.CreateField(), () => new T());
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) =>
        infoType.IsList() &&
        infoType.GenericTypeArguments.Length > 0 &&
        infoType.GenericTypeArguments[0].IsSubclassOf(typeof(SharedVariable));

    }
    public class SharedVariableListField<T> : ListField<T>, IInitable where T : SharedVariable
    {
        private IDialogueTreeView treeView;
        private Action<IDialogueTreeView> OnTreeViewInitEvent;
        public SharedVariableListField(string label, VisualElement visualInput, Func<VisualElement> elementCreator, Func<object> valueCreator) : base(label, visualInput, elementCreator, valueCreator)
        {

        }
        public void Init(IDialogueTreeView treeView)
        {
            this.treeView = treeView;
            OnTreeViewInitEvent?.Invoke(treeView);
        }
        protected override ListView CreateListView()
        {
            void BindItem(VisualElement e, int i)
            {
                (e as BaseField<T>).value = value[i];
                (e as BaseField<T>).RegisterValueChangedCallback((x) => value[i] = x.newValue);
            }
            VisualElement MakeItem()
            {
                var field = elementCreator.Invoke();
                (field as BaseField<T>).label = string.Empty;
                if (treeView != null) (field as IInitable).Init(treeView);
                OnTreeViewInitEvent += (view) => { (field as IInitable).Init(view); };
                return field;
            }
            var view = new ListView(value, 60, MakeItem, BindItem);
            return view;
        }

    }
}