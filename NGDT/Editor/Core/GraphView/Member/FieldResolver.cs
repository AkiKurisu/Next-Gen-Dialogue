using System;
using System.Reflection;
using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;
namespace Kurisu.NGDT.Editor
{
    /// <summary>
    /// Give priority for resolver (node or field)
    /// </summary>
    public sealed class Ordered : Attribute
    {
        public int Order = 100;
    }
    public delegate void ValueChangeDelegate(object value);
    public interface IFieldResolver
    {
        /// <summary>
        /// Get the ValueField and bind the tree view at the same time
        /// </summary>
        /// <param name="ownerTreeView"></param>
        /// <returns></returns>
        VisualElement GetEditorField(IDialogueTreeView ownerTreeView);
        /// <summary>
        /// Get the ValueField and bind the shared variable at the same time
        /// </summary>
        /// <param name="ExposedProperties"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        VisualElement GetEditorField(List<SharedVariable> ExposedProperties, SharedVariable variable);
        /// <summary>
        /// Only create ValueField without any binding
        /// </summary>
        /// <returns></returns>
        public VisualElement CreateField();
        void Restore(object behavior);
        void Commit(object behavior);
        void Copy(IFieldResolver resolver);
        object Value { get; set; }
        /// <summary>
        /// Register an object value change call back without knowing it's type
        /// </summary>
        /// <param name="fieldChangeCallBack"></param>
        void RegisterValueChangeCallback(ValueChangeDelegate fieldChangeCallBack);
    }

    public abstract class FieldResolver<T, K> : IFieldResolver where T : BaseField<K>
    {
        private readonly FieldInfo fieldInfo;
        private T editorField;
        public T EditorField => editorField;
        public object Value { get => editorField.value; set => editorField.SetValueWithoutNotify((K)value); }
        protected FieldResolver(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            SetEditorField();
        }
        private void SetEditorField()
        {
            editorField = CreateEditorField(fieldInfo);
            //修改标签
            AkiLabelAttribute label = fieldInfo.GetCustomAttribute<AkiLabelAttribute>();
            if (label != null) editorField.label = label.Title;
            TooltipAttribute tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            if (tooltip != null) editorField.tooltip = tooltip.tooltip;
        }

        protected abstract T CreateEditorField(FieldInfo fieldInfo);
        public VisualElement CreateField() => CreateEditorField(fieldInfo);
        protected virtual void SetTree(IDialogueTreeView ownerTreeView) { }
        public VisualElement GetEditorField(IDialogueTreeView ownerTreeView)
        {
            SetTree(ownerTreeView);
            return editorField;
        }
        public VisualElement GetEditorField(List<SharedVariable> ExposedProperties, SharedVariable variable)
        {
            editorField.RegisterValueChangedCallback(evt =>
            {
                var index = ExposedProperties.FindIndex(x => x.Name == variable.Name);
                ExposedProperties[index].SetValue(evt.newValue);
            });
            editorField.value = (K)variable.GetValue();
            return editorField;
        }
        public void Copy(IFieldResolver resolver)
        {
            if (resolver is not FieldResolver<T, K>) return;
            if (fieldInfo.GetCustomAttribute<CopyDisableAttribute>() != null) return;
            editorField.value = (K)resolver.Value;
        }
        public void Restore(object behavior)
        {
            editorField.value = (K)fieldInfo.GetValue(behavior);
        }
        public void Commit(object behavior)
        {
            fieldInfo.SetValue(behavior, editorField.value);
        }
        public void RegisterValueChangeCallback(ValueChangeDelegate fieldChangeCallBack)
        {
            editorField.RegisterValueChangedCallback(evt => fieldChangeCallBack?.Invoke(evt.newValue));
        }
    }
}