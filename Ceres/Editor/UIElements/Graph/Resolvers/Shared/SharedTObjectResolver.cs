using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public class SharedTObjectResolver<T> : FieldResolver<SharedTObjectField<T>, SharedTObject<T>> where T : UnityEngine.Object
    {
        public SharedTObjectResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {

        }
        protected override SharedTObjectField<T> CreateEditorField(FieldInfo fieldInfo)
        {
            return new SharedTObjectField<T>(fieldInfo.Name, null, fieldInfo);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _)
        {
            return infoType.IsSharedTObject() && infoType.GenericTypeArguments.Length == 1 && infoType.GenericTypeArguments[0] == typeof(T);
        }
    }
    public class SharedTObjectField<T> : BaseField<SharedTObject<T>>, IBindableField where T : UnityEngine.Object
    {
        private readonly bool forceShared;
        private readonly VisualElement foldout;
        private readonly Toggle toggle;
        private CeresGraphView graphView;
        private DropdownField nameDropdown;
        private SharedVariable bindExposedProperty;
        public SharedTObjectField(string label, VisualElement visualInput, FieldInfo fieldInfo) : base(label, visualInput)
        {
            forceShared = fieldInfo.GetCustomAttribute<ForceSharedAttribute>() != null;
            AddToClassList("SharedVariableField");
            foldout = new VisualElement();
            foldout.style.flexDirection = FlexDirection.Row;
            contentContainer.Add(foldout);
            toggle = new Toggle("Is Shared");
            toggle.RegisterValueChangedCallback(evt => { value.IsShared = evt.newValue; OnToggle(evt.newValue); NotifyValueChange(); });
            if (forceShared)
            {
                toggle.value = true;
                return;
            }
            foldout.Add(toggle);
        }
        public void BindGraph(CeresGraphView graphView)
        {
            this.graphView = graphView;
            graphView.Blackboard.View.RegisterCallback<VariableChangeEvent>(evt =>
            {
                if (evt.ChangeType != VariableChangeType.NameChange) return;
                if (evt.Variable != bindExposedProperty) return;
                nameDropdown.value = value.Name = evt.Variable.Name;
            });
            OnToggle(toggle.value);
        }
        private static List<string> GetList(CeresGraphView graphView)
        {
            return graphView.SharedVariables
            .Where(x => x is SharedObject sharedObject && sharedObject.ConstraintTypeAQN == typeof(T).AssemblyQualifiedName)
            .Select(v => v.Name)
            .ToList();
        }
        private void BindProperty()
        {
            if (graphView == null) return;
            bindExposedProperty = graphView.SharedVariables
                .FirstOrDefault(x => x is SharedObject sharedObject && sharedObject.ConstraintTypeAQN == typeof(T).AssemblyQualifiedName && x.Name.Equals(value.Name));
        }
        private void OnToggle(bool IsShared)
        {
            if (IsShared)
            {
                RemoveNameDropDown();
                if (value != null && graphView != null) AddNameDropDown();
                RemoveValueField();
            }
            else
            {
                RemoveNameDropDown();
                RemoveValueField();
                AddValueField();
            }
        }
        private void AddNameDropDown()
        {
            var list = GetList(graphView);
            value.Name = value.Name ?? string.Empty;
            int index = list.IndexOf(value.Name);
            nameDropdown = new DropdownField($"Shared {typeof(T).Name}", list, index);
            nameDropdown.RegisterCallback<MouseEnterEvent>((evt) => { nameDropdown.choices = GetList(graphView); });
            nameDropdown.RegisterValueChangedCallback(evt => { value.Name = evt.newValue; BindProperty(); NotifyValueChange(); });
            foldout.Insert(0, nameDropdown);
        }
        private void RemoveNameDropDown()
        {
            if (nameDropdown != null) foldout.Remove(nameDropdown);
            nameDropdown = null;
        }
        private void RemoveValueField()
        {
            if (ValueField != null) foldout.Remove(ValueField);
            ValueField = null;
        }
        private void AddValueField()
        {
            ValueField = new ObjectField()
            {
                objectType = typeof(T)
            };
            ValueField.RegisterValueChangedCallback(evt => { value.Value = (T)evt.newValue; NotifyValueChange(); });
            if (value != null) ValueField.value = value.Value;
            foldout.Insert(0, ValueField);
        }
        public sealed override SharedTObject<T> value
        {
            get => base.value; set
            {
                if (value != null) base.value = value.Clone() as SharedTObject<T>;
                else base.value = new SharedTObject<T>();
                if (forceShared)
                {
                    var sharedTObject = base.value;
                    if (sharedTObject != null)
                        sharedTObject.IsShared = true;
                }

                Repaint();
            }
        }
        private ObjectField ValueField { get; set; }
        public void Repaint()
        {
            toggle.value = value.IsShared;
            if (ValueField != null) ValueField.value = value.Value;
            BindProperty();
            OnToggle(value.IsShared);
            NotifyValueChange();
        }
        protected void NotifyValueChange()
        {
            using ChangeEvent<SharedTObject<T>> changeEvent = ChangeEvent<SharedTObject<T>>.GetPooled(value, value);
            changeEvent.target = this;
            SendEvent(changeEvent);
        }
    }
}
