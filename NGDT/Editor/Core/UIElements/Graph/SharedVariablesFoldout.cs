using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres.Editor.Graph;
using Ceres.Graph;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    // TODO: Refactor
    public class SharedVariablesFoldout : Foldout
    {
        private readonly HashSet<ObserveProxyVariable> _observeProxies;
        public SharedVariablesFoldout(IVariableSource source, System.Action onUpdate)
        {
            value = false;
            text = "SharedVariables";
            RegisterCallback<DetachFromPanelEvent>(Release);
            _observeProxies = new HashSet<ObserveProxyVariable>();
            var factory = FieldResolverFactory.Get();
            foreach (var variable in source.SharedVariables.Where(x => x.IsExposed))
            {
                if (variable is PieceID) continue;
                var grid = new Foldout
                {
                    text = $"{variable.GetType().Name}  :  {variable.Name}",
                    value = false
                };
                var content = new VisualElement
                {
                    style =
                    {
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.SpaceBetween
                    }
                };
                var fieldResolver = factory.Create(variable.GetType().GetField("value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public));
                var valueField = fieldResolver.EditorField;
                fieldResolver.Restore(variable);
                fieldResolver.RegisterValueChangeCallback((obj) =>
                {
                    var index = source.SharedVariables.FindIndex(x => x.Name == variable.Name);
                    source.SharedVariables[index].SetValue(obj);
                    onUpdate();
                });
                if (Application.isPlaying)
                {
                    var observeProxy = variable.Observe();
                    observeProxy.Register(x => fieldResolver.Value = x);
                    fieldResolver.Value = variable.GetValue();
                    //Disable since you should only edit global variable in source
                    if (variable.IsGlobal) valueField.SetEnabled(false);
                    valueField.tooltip = "Global variable can only be edited in source at runtime";
                    _observeProxies.Add(observeProxy);
                }
                if (valueField is TextField field)
                {
                    field.multiline = true;
                    field.style.maxWidth = 250;
                    field.style.whiteSpace = WhiteSpace.Normal;
                }
                valueField.style.width = Length.Percent(70f);
                content.Add(valueField);
                if (variable is SharedUObject sharedObject)
                {
                    var objectField = (ObjectField)valueField;
                    objectField.objectType = sharedObject.GetValueType();
                    grid.text = $"{variable.GetType().Name} ({objectField.objectType.Name}):  {variable.Name}";
                }
                //Is Global Field
                var globalToggle = new Button()
                {
                    text = "Is Global"
                };
                if (!Application.isPlaying)
                {
                    globalToggle.clicked += () =>
                    {
                        variable.IsGlobal = !variable.IsGlobal;
                        SetToggleButtonColor(globalToggle, variable.IsGlobal);
                        onUpdate();
                    };
                }
                SetToggleButtonColor(globalToggle, variable.IsGlobal);
                globalToggle.style.width = Length.Percent(15);
                globalToggle.style.height = 25;
                content.Add(globalToggle);
                // Delete Variable
                if (!Application.isPlaying)
                {
                    var deleteButton = new Button(() =>
                    {
                        source.SharedVariables.Remove(variable);
                        Remove(grid);
                        if (source.SharedVariables.Count == 0)
                        {
                            RemoveFromHierarchy();
                        }
                        onUpdate();
                    })
                    {
                        text = "Delete",
                        style =
                        {
                            width = Length.Percent(10f),
                            height = 25
                        }
                    };
                    content.Add(deleteButton);
                }
                //Append to row
                grid.Add(content);
                //Append to folder
                Add(grid);
            }

            return;
            
            void SetToggleButtonColor(Button button, bool isOn)
            {
                button.style.color = isOn ? Color.green : Color.gray;
            }
        }

        private void Release(DetachFromPanelEvent _)
        {
            foreach (var proxy in _observeProxies)
            {
                proxy.Dispose();
            }
        }
    }
}
