using System.Reflection;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class EnumListResolver<T> : FieldResolver<EnumListField<T>, List<T>> where T : Enum
    {
        protected readonly IFieldResolver childResolver;
        public EnumListResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
            childResolver = new EnumResolver(fieldInfo);
        }
        protected override EnumListField<T> CreateEditorField(FieldInfo fieldInfo)
        {
            return new EnumListField<T>(fieldInfo.Name, null, () => childResolver.CreateField(), () => default(T));
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) =>
        infoType.IsList() && infoType.GenericTypeArguments.Length > 0 && infoType.GenericTypeArguments[0].IsEnum;

    }
    public class EnumListField<T> : ListField<T> where T : Enum
    {
        public EnumListField(string label, VisualElement visualInput, Func<VisualElement> elementCreator, Func<object> valueCreator) : base(label, visualInput, elementCreator, valueCreator)
        {

        }
        protected override ListView CreateListView()
        {
            void BindItem(VisualElement e, int i)
            {
                (e as EnumField).value = value[i];
                (e as EnumField).RegisterValueChangedCallback((x) => value[i] = (T)x.newValue);
            }
            VisualElement MakeItem()
            {
                var field = elementCreator.Invoke();
                (field as EnumField).label = string.Empty;
                return field;
            }
            const int itemHeight = 20;
            var view = new ListView(value, itemHeight, MakeItem, BindItem);
            return view;
        }

    }
}