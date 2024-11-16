using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Ceres.Editor
{
    public class CeresBlackboard : Blackboard
    {
        public Blackboard View => this;
        public bool AlwaysExposed { get; set; }
        
        protected readonly FieldResolverFactory fieldResolverFactory = FieldResolverFactory.Instance;
        
        protected readonly ScrollView scrollView;
        
        protected readonly List<SharedVariable> sharedVariables;
        
        protected readonly HashSet<ObserveProxyVariable> observeProxies = new();
        public CeresBlackboard(IVariableSource variableSource, GraphView graphView) : base(graphView)
        {
            var header = this.Q("header");
            header.style.height = new StyleLength(50);
            Add(scrollView = new());
            scrollView.Add(new BlackboardSection { title = "Shared Variables" });
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            RegisterCallback<DetachFromPanelEvent>(OnDispose);
            sharedVariables = variableSource.SharedVariables;
            if (!Application.isPlaying) InitRequestDelegate();
        }
        private void OnDispose(DetachFromPanelEvent _)
        {
            foreach (var proxy in observeProxies)
            {
                proxy.Dispose();
            }
        }
        private void InitRequestDelegate()
        {
            addItemRequested = _blackboard =>
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Int"), false, () => AddVariable(new SharedInt(), true));
                    menu.AddItem(new GUIContent("Float"), false, () => AddVariable(new SharedFloat(), true));
                    menu.AddItem(new GUIContent("Bool"), false, () => AddVariable(new SharedBool(), true));
                    menu.AddItem(new GUIContent("Vector3"), false, () => AddVariable(new SharedVector3(), true));
                    menu.AddItem(new GUIContent("String"), false, () => AddVariable(new SharedString(), true));
                    menu.AddItem(new GUIContent("Object"), false, () => AddVariable(new SharedObject(), true));
                    menu.ShowAsContext();
                };
            editTextRequested = (_blackboard, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                var index = sharedVariables.FindIndex(x => x.Name == oldPropertyName);
                if (string.IsNullOrEmpty(newValue))
                {
                    scrollView.RemoveAt(index + 1);
                    sharedVariables.RemoveAt(index);
                    return;
                }
                if (sharedVariables.Any(x => x.Name == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "A variable with the same name already exists !",
                        "OK");
                    return;
                }
                var targetIndex = sharedVariables.FindIndex(x => x.Name == oldPropertyName);
                sharedVariables[targetIndex].Name = newValue;
                NotifyVariableChanged(sharedVariables[targetIndex], VariableChangeType.NameChange);
                ((BlackboardField)element).text = newValue;
            };

        }
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            contentContainer.style.height = layout.height - 50;
        }
        public void EditVariable(string variableName)
        {
            var index = sharedVariables.FindIndex(x => x.Name == variableName);
            if (index < 0) return;
            var field = scrollView.Query<BlackboardField>().AtIndex(index);
            scrollView.ScrollTo(field);
            field.OpenTextEditor();
        }
        public void AddVariable(SharedVariable variable, bool fireEvents)
        {
            var localPropertyValue = variable.GetValue();
            if (string.IsNullOrEmpty(variable.Name)) variable.Name = variable.GetType().Name;
            var localPropertyName = variable.Name;
            int index = 1;
            while (sharedVariables.Any(x => x.Name == localPropertyName))
            {
                localPropertyName = $"{variable.Name}{index++}";
            }
            variable.Name = localPropertyName;
            if (AlwaysExposed) variable.IsExposed = true;
            sharedVariables.Add(variable);
            var container = new VisualElement();
            var field = new BlackboardField { text = localPropertyName, typeText = variable.GetType().Name };
            field.capabilities &= ~Capabilities.Deletable;
            field.capabilities &= ~Capabilities.Movable;
            if (Application.isPlaying)
            {
                field.capabilities &= ~Capabilities.Renamable;
            }
            FieldInfo info = variable.GetType().GetField("value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var fieldResolver = fieldResolverFactory.Create(info);
            var valueField = fieldResolver.GetEditorField(null);
            fieldResolver.Restore(variable);
            fieldResolver.RegisterValueChangeCallback((obj) =>
            {
                var index = sharedVariables.FindIndex(x => x.Name == variable.Name);
                sharedVariables[index].SetValue(obj);
                NotifyVariableChanged(variable, VariableChangeType.ValueChange);
            });
            if (Application.isPlaying)
            {
                var observe = variable.Observe();
                observe.Register(x => fieldResolver.Value = x);
                observeProxies.Add(observe);
                fieldResolver.Value = variable.GetValue();
                //Disable since you should only edit global variable in source
                if (variable.IsGlobal)
                {
                    valueField.SetEnabled(false);
                    valueField.tooltip = "Global variable can only edited in source at runtime";
                }
            }
            var row = CreateBlackboardPropertyRow(variable, field, valueField);
            scrollView.Add(row);
            if (fireEvents) NotifyVariableChanged(variable, VariableChangeType.Create);
        }

        protected virtual BlackboardRow CreateBlackboardPropertyRow(SharedVariable variable, BlackboardField blackboardField, VisualElement valueField)
        {
            var propertyView = new VisualElement();
            if (!AlwaysExposed)
            {
                var toggle = new Toggle("Exposed")
                {
                    value = variable.IsExposed
                };
                if (Application.isPlaying)
                {
                    toggle.SetEnabled(false);
                }
                else
                {
                    toggle.RegisterValueChangedCallback(x =>
                    {
                        var index = sharedVariables.FindIndex(x => x.Name == variable.Name);
                        sharedVariables[index].IsExposed = x.newValue;
                        NotifyVariableChanged(variable, VariableChangeType.ValueChange);
                    });
                }
                propertyView.Add(toggle);
            }
            propertyView.Add(valueField);
            if (variable is SharedObject sharedObject)
            {
                propertyView.Add(GetConstraintField(sharedObject, (ObjectField)valueField));
            }
            var sa = new BlackboardRow(blackboardField, propertyView);
            sa.AddManipulator(new ContextualMenuManipulator((evt) => BuildBlackboardMenu(evt, variable)));
            return sa;
        }
        public void RemoveVariable(SharedVariable variable, bool fireEvents)
        {
            var index = sharedVariables.FindIndex(x => x == variable);
            if (index < 0) return;
            var row = scrollView.Query<BlackboardRow>().AtIndex(index);
            row.RemoveFromHierarchy();
            sharedVariables.Remove(variable);
            if (fireEvents) NotifyVariableChanged(variable, VariableChangeType.Delete);
        }

        protected void NotifyVariableChanged(SharedVariable sharedVariable, VariableChangeType changeType)
        {
            using VariableChangeEvent changeEvent = VariableChangeEvent.GetPooled(sharedVariable, changeType);
            changeEvent.target = this;
            SendEvent(changeEvent);
        }

        protected virtual void BuildBlackboardMenu(ContextualMenuPopulateEvent evt, SharedVariable variable)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Delete", (a) =>
            {
                RemoveVariable(variable, true);
            }));
            evt.menu.MenuItems().Add(new CeresDropdownMenuAction("Duplicate", (a) =>
           {
               AddVariable(variable.Clone(), true);
           }));
        }
        protected static VisualElement GetConstraintField(SharedObject sharedObject, ObjectField objectField)
        {
            const string NonConstraint = "No Constraint";
            var placeHolder = new VisualElement();
            string constraintTypeName;
            try
            {
                objectField.objectType = Type.GetType(sharedObject.ConstraintTypeAQN, true);
                constraintTypeName = "Constraint Type : " + objectField.objectType.Name;
            }
            catch
            {
                objectField.objectType = typeof(UnityEngine.Object);
                constraintTypeName = NonConstraint;
            }
            var typeField = new Label(constraintTypeName);
            placeHolder.Add(typeField);
            var button = new Button()
            {
                text = "Change Constraint Type"
            };
            button.clicked += () =>
             {
                 var provider = ScriptableObject.CreateInstance<ObjectTypeSearchWindow>();
                 provider.Initialize((type) =>
                 {
                     if (type == null)
                     {
                         typeField.text = sharedObject.ConstraintTypeAQN = NonConstraint;
                         objectField.objectType = typeof(UnityEngine.Object);
                     }
                     else
                     {
                         objectField.objectType = type;
                         sharedObject.ConstraintTypeAQN = type.AssemblyQualifiedName;
                         typeField.text = "Constraint Type : " + type.Name;
                     }
                 });
                 SearchWindow.Open(new SearchWindowContext(GUIUtility.GUIToScreenPoint(Event.current.mousePosition)), provider);
             };

            placeHolder.Add(button);
            return placeHolder;
        }
    }
}