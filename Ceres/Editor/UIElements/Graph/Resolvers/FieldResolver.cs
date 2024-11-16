using System.Reflection;
using UnityEngine.UIElements;
using UnityEngine;
using System;
using Ceres.Annotations;
namespace Ceres.Editor
{
    public delegate void ValueChangeDelegate(object value);
    public interface IBindableField
    {
        void BindGraph(CeresGraphView ceresGraphView);
    }
    public interface IFieldResolver
    {
        /// <summary>
        /// Get the ValueField and bind the tree view at the same time
        /// </summary>
        /// <param name="ceresGraphView"></param>
        /// <returns></returns>
        VisualElement GetEditorField(CeresGraphView ceresGraphView);
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
        protected readonly T editorField;
        public T EditorField => editorField;
        public virtual object Value
        {
            get => editorField.value;
            set => editorField.value = (K)value;
        }
        protected FieldResolver(FieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
            editorField = CreateEditorField(fieldInfo);
            NodeLabelAttribute label = fieldInfo.GetCustomAttribute<NodeLabelAttribute>();
            if (label != null) editorField.label = label.Title;
            TooltipAttribute tooltip = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            if (tooltip != null) editorField.tooltip = tooltip.tooltip;
        }
        public VisualElement CreateField()
        {
            return CreateEditorField(fieldInfo);
        }
        public VisualElement GetEditorField(CeresGraphView ceresGraphView)
        {
            if (editorField is IBindableField bindableField) bindableField.BindGraph(ceresGraphView);
            return editorField;
        }
        public void Copy(IFieldResolver resolver)
        {
            if (resolver is not FieldResolver<T, K>) return;
            if (fieldInfo.GetCustomAttribute<CopyDisableAttribute>() != null) return;
            Value = resolver.Value;
        }
        public void Restore(object behavior)
        {
            Value = fieldInfo.GetValue(behavior);
        }
        public void Commit(object behavior)
        {
            fieldInfo.SetValue(behavior, Value);
        }
        public void RegisterValueChangeCallback(ValueChangeDelegate fieldChangeCallBack)
        {
            editorField.RegisterValueChangedCallback(evt => fieldChangeCallBack?.Invoke(evt.newValue));
        }
        /// <summary>
        /// Create <see cref="BaseField{T}"/>
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns></returns>
        protected abstract T CreateEditorField(FieldInfo fieldInfo);
    }
    public abstract class FieldResolver<T, K, F> : FieldResolver<T, K> where T : BaseField<K> where K : F
    {
        protected FieldResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }

        public sealed override object Value
        {
            get => ValueGetter != null ? ValueGetter(editorField.value) : editorField.value;
            set => editorField.value = ValueSetter != null ? ValueSetter((F)value) : (K)value;
        }

        /// <summary>
        /// Bridge for setting value from <see cref="K"/> to <see cref="F"/>
        /// </summary>
        /// <value></value>
        protected Func<F, K> ValueSetter { get; set; }
        /// <summary>
        /// Bridge for setting value from <see cref="K"/> to <see cref="F"/>
        /// </summary>
        /// <value></value>
        protected Func<K, object> ValueGetter { get; set; }
    }
}