using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    internal interface IInitable
    {
        void Init(IDialogueTreeView treeView);
    }
    public abstract class SharedVariableField<T, K> : BaseField<T>, IInitable where T : SharedVariable<K>, new()
    {
        private readonly bool forceShared;
        private readonly VisualElement sharedVariableContainer;
        private readonly Toggle toggle;
        private IDialogueTreeView treeView;
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
        public void Init(IDialogueTreeView treeView)
        {
            this.treeView = treeView;
            treeView.OnPropertyNameChange += (variable) =>
            {
                if (variable != bindExposedProperty) return;
                nameDropdown.value = variable.Name;
                value.Name = variable.Name;
            };
            OnToggle(toggle.value);
        }
        private static List<string> GetList(IDialogueTreeView treeView)
        {
            return treeView.ExposedProperties
            .Where(x => x.GetType() == typeof(T))
            .Select(v => v.Name)
            .ToList();
        }
        private void BindProperty()
        {
            if (treeView == null) return;
            bindExposedProperty = treeView.ExposedProperties.Where(x => x.GetType() == typeof(T) && x.Name.Equals(value.Name)).FirstOrDefault();
        }
        private void OnToggle(bool IsShared)
        {
            if (IsShared)
            {
                RemoveNameDropDown();
                if (value != null && treeView != null) AddNameDropDown();
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
            var list = GetList(treeView);
            value.Name = value.Name ?? string.Empty;
            int index = list.IndexOf(value.Name);
            nameDropdown = new DropdownField(bindType.Name, list, index);
            nameDropdown.RegisterCallback<MouseEnterEvent>((evt) => { nameDropdown.choices = GetList(treeView); });
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
            }
        }
        protected BaseField<K> ValueField { get; set; }
        public void Repaint()
        {
            toggle.value = value.IsShared;
            if (ValueField != null) ValueField.value = value.Value;
            BindProperty();
            OnToggle(value.IsShared);
            NotifyValueChange();
            OnValueUpdate();
        }
        protected void NotifyValueChange()
        {
            using ChangeEvent<T> changeEvent = ChangeEvent<T>.GetPooled(value, value);
            changeEvent.target = this;
            SendEvent(changeEvent);
        }
        protected virtual void OnValueUpdate() { }
    }

}

;