using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Annotations;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public abstract class SharedVariableField<T, K> : BaseField<T>, IBindableField where T : SharedVariable<K>, new()
    {
        private readonly bool forceShared;
        private readonly VisualElement sharedVariableContainer;
        private readonly Toggle toggle;
        private CeresGraphView graphView;
        private DropdownField nameDropdown;
        private SharedVariable bindExposedProperty;
        private readonly Type bindType;
        public SharedVariableField(string label, VisualElement visualInput, Type objectType, FieldInfo fieldInfo) : base(label, visualInput)
        {
            forceShared = fieldInfo.GetCustomAttribute<ForceSharedAttribute>() != null;
            AddToClassList("SharedVariableField");
            sharedVariableContainer = new VisualElement();
            sharedVariableContainer.style.flexDirection = FlexDirection.Row;
            contentContainer.Add(sharedVariableContainer);
            bindType = objectType;
            toggle = new Toggle("Is Shared");
            toggle.RegisterValueChangedCallback(evt => { value.IsShared = evt.newValue; OnToggle(evt.newValue); NotifyValueChange(); });
            if (forceShared)
            {
                toggle.value = true;
                return;
            }
            sharedVariableContainer.Add(toggle);
        }
        public void BindGraph(CeresGraphView graphView)
        {
            this.graphView = graphView;
            this.graphView.Blackboard.View.RegisterCallback<VariableChangeEvent>(OnVariableChange);
            OnToggle(toggle.value);
        }
        private void OnVariableChange(VariableChangeEvent evt)
        {
            if (evt.ChangeType != VariableChangeType.NameChange) return;
            if (evt.Variable != bindExposedProperty) return;
            nameDropdown.value = value.Name = evt.Variable.Name;
        }
        private static List<string> GetList(CeresGraphView graphView)
        {
            return graphView.SharedVariables
            .Where(x => x.GetType() == typeof(T))
            .Select(v => v.Name)
            .ToList();
        }
        private void BindProperty()
        {
            if (graphView == null) return;
            bindExposedProperty = graphView.SharedVariables.Where(x => x.GetType() == typeof(T) && x.Name.Equals(value.Name)).FirstOrDefault();
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
            nameDropdown = new DropdownField(bindType.Name, list, index);
            nameDropdown.RegisterCallback<MouseEnterEvent>((evt) => { nameDropdown.choices = GetList(graphView); });
            nameDropdown.RegisterValueChangedCallback(evt => { value.Name = evt.newValue; BindProperty(); NotifyValueChange(); });
            sharedVariableContainer.Insert(0, nameDropdown);
        }
        private void RemoveNameDropDown()
        {
            if (nameDropdown != null) sharedVariableContainer.Remove(nameDropdown);
            nameDropdown = null;
        }
        private void RemoveValueField()
        {
            if (ValueField != null) sharedVariableContainer.Remove(ValueField);
            ValueField = null;
        }
        private void AddValueField()
        {
            ValueField = CreateValueField();
            ValueField.RegisterValueChangedCallback(evt => { value.Value = evt.newValue; NotifyValueChange(); });
            if (value != null) ValueField.value = value.Value;
            sharedVariableContainer.Insert(0, ValueField);
        }
        protected abstract BaseField<K> CreateValueField();
        public sealed override T value
        {
            get => base.value; set
            {
                if (value != null) base.value = value.Clone() as T;
                else base.value = new T();
                if (forceShared) base.value.IsShared = true;
                Repaint();
                NotifyValueChange();
            }
        }
        public BaseField<K> ValueField { get; protected set; }
        public void SetValue(K newValue)
        {
            value.Value = newValue;
            if (ValueField != null) ValueField.value = value.Value;
        }
        public void Repaint()
        {
            toggle.value = value.IsShared;
            if (ValueField != null) ValueField.value = value.Value;
            BindProperty();
            OnToggle(value.IsShared);
            OnRepaint();
        }
        protected void NotifyValueChange()
        {
            using ChangeEvent<T> changeEvent = ChangeEvent<T>.GetPooled(value, value);
            changeEvent.target = this;
            SendEvent(changeEvent);
        }
        protected virtual void OnRepaint() { }
    }

}

;