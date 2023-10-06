using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class AdvancedBlackBoard : Blackboard, IBlackBoard
    {
        private readonly FieldResolverFactory fieldResolverFactory = FieldResolverFactory.Instance;
        private readonly ScrollView scrollView;
        public VisualElement RawContainer => scrollView;
        private readonly List<SharedVariable> exposedProperties;
        public AdvancedBlackBoard(DialogueTreeView treeView) : base(treeView)
        {
            var header = this.Q("header");
            header.style.height = new StyleLength(50);
            Add(scrollView = new());
            scrollView.Add(new BlackboardSection { title = "Shared Variables" });
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            exposedProperties = treeView.ExposedProperties;
            InitRequestDelegate(treeView);
        }
        private void InitRequestDelegate(DialogueTreeView _graphView)
        {
            addItemRequested = _blackboard =>
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Int"), false, () => AddExposedProperty(new SharedInt(), true));
                    menu.AddItem(new GUIContent("Float"), false, () => AddExposedProperty(new SharedFloat(), true));
                    menu.AddItem(new GUIContent("Bool"), false, () => AddExposedProperty(new SharedBool(), true));
                    menu.AddItem(new GUIContent("Vector3"), false, () => AddExposedProperty(new SharedVector3(), true));
                    menu.AddItem(new GUIContent("String"), false, () => AddExposedProperty(new SharedString(), true));
                    menu.AddItem(new GUIContent("Object"), false, () => AddExposedProperty(new SharedObject(), true));
                    menu.ShowAsContext();
                };
            editTextRequested = (_blackboard, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                var index = _graphView.ExposedProperties.FindIndex(x => x.Name == oldPropertyName);
                if (string.IsNullOrEmpty(newValue))
                {
                    RawContainer.RemoveAt(index + 1);
                    _graphView.ExposedProperties.RemoveAt(index);
                    return;
                }
                if (_graphView.ExposedProperties.Any(x => x.Name == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "A variable with the same name already exists !",
                        "OK");
                    return;
                }
                var targetIndex = _graphView.ExposedProperties.FindIndex(x => x.Name == oldPropertyName);
                _graphView.ExposedProperties[targetIndex].Name = newValue;
                _graphView.NotifyEditSharedVariable(_graphView.ExposedProperties[targetIndex]);
                ((BlackboardField)element).text = newValue;
            };

        }
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            contentContainer.style.height = layout.height - 50;
        }
        public void AddExposedProperty(SharedVariable variable, bool canDuplicate)
        {
            var localPropertyValue = variable.GetValue();
            if (string.IsNullOrEmpty(variable.Name)) variable.Name = variable.GetType().Name;
            var localPropertyName = variable.Name;
            int index = 1;
            while (exposedProperties.Any(x => x.Name == localPropertyName))
            {
                if (!canDuplicate) return;
                localPropertyName = $"{variable.Name}{index++}";
            }
            variable.Name = localPropertyName;
            exposedProperties.Add(variable);
            var container = new VisualElement();
            var field = new BlackboardField { text = localPropertyName, typeText = variable.GetType().Name };
            field.capabilities &= ~Capabilities.Deletable;
            field.capabilities &= ~Capabilities.Movable;
            FieldInfo info = variable.GetType().GetField("value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            var fieldResolver = fieldResolverFactory.Create(info);
            var valueField = fieldResolver.GetEditorField(exposedProperties, variable);
            var placeHolder = new VisualElement();
            placeHolder.Add(valueField);
            if (variable is SharedObject sharedObject)
            {
                placeHolder.Add(GetConstraintField(sharedObject, (ObjectField)valueField));
            }
            var sa = new BlackboardRow(field, placeHolder);
            sa.AddManipulator(new ContextualMenuManipulator((evt) => BuildBlackboardMenu(evt, sa)));
            RawContainer.Add(sa);
        }
        private void BuildBlackboardMenu(ContextualMenuPopulateEvent evt, VisualElement element)
        {
            evt.menu.MenuItems().Clear();
            evt.menu.MenuItems().Add(new NGDTDropdownMenuAction("Delate Variable", (a) =>
            {
                int index = RawContainer.IndexOf(element);
                exposedProperties.RemoveAt(index - 1);
                RawContainer.Remove(element);
                return;
            }));
        }
        private VisualElement GetConstraintField(SharedObject sharedObject, ObjectField objectField)
        {
            const string NonConstraint = "No Constraint";
            var placeHolder = new VisualElement();
            string constraintTypeName;
            try
            {
                objectField.objectType = Type.GetType(sharedObject.ConstraintTypeAQM, true);
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
                         typeField.text = sharedObject.ConstraintTypeAQM = NonConstraint;
                         objectField.objectType = typeof(UnityEngine.Object);
                     }
                     else
                     {
                         objectField.objectType = type;
                         sharedObject.ConstraintTypeAQM = type.AssemblyQualifiedName;
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