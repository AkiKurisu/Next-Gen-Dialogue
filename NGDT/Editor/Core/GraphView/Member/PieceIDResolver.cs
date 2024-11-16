using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres;
using Ceres.Editor;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    [Ordered]
    public class PieceIDResolver : FieldResolver<PieceIDField, PieceID>
    {
        public PieceIDResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override PieceIDField CreateEditorField(FieldInfo fieldInfo)
        {
            bool isReferenced = fieldInfo.GetCustomAttribute<ReferencePieceIDAttribute>() != null;
            return new PieceIDField(fieldInfo.Name, isReferenced);
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(PieceID);

    }
    public class PieceIDField : BaseField<PieceID>, IBindableField
    {
        private CeresGraphView graphView;
        private DropdownField nameDropdown;
        internal SharedVariable bindVariable;
        private readonly bool isReferenced;
        public PieceIDField(string label, bool isReferenced) : base(label, null)
        {
            AddToClassList("SharedVariableField");
            this.isReferenced = isReferenced;
            if (!isReferenced) AddToClassList("PieceIDField");
        }
        public void BindGraph(CeresGraphView graphView)
        {
            this.graphView = graphView;
            this.graphView.Blackboard.View.RegisterCallback<VariableChangeEvent>(evt =>
            {
                if (evt.ChangeType != VariableChangeType.NameChange) return;
                if (evt.Variable != bindVariable) return;
                UpdateID(evt.Variable);
            });
            if (value != null)
            {
                BindProperty();
                UpdateValueField();
            }
        }
        private void UpdateID(SharedVariable variable)
        {
            if (isReferenced)
            {
                nameDropdown.value = variable.Name;
            }
            else label = variable.Name;
            value.Name = variable.Name;
        }
        private static List<string> GetList(CeresGraphView graphView)
        {
            return graphView.SharedVariables
                        .Where(x => x.GetType() == typeof(PieceID))
                        .Select(v => v.Name)
                        .ToList();
        }
        private void BindProperty()
        {
            bindVariable = graphView.SharedVariables.FirstOrDefault(x => x.GetType() == typeof(PieceID) && x.Name.Equals(value.Name));
        }
        private void UpdateValueField()
        {
            if (isReferenced)
            {
                if (nameDropdown == null) AddDropDown();
                else nameDropdown.value = value.Name;
            }
            else
            {
                label = value.Name;
            }
        }
        private void AddDropDown()
        {
            var list = GetList(graphView);
            value.Name = value.Name ?? string.Empty;
            int index = list.IndexOf(value.Name);
            nameDropdown = new DropdownField("Piece ID", list, index);
            nameDropdown.RegisterCallback<MouseEnterEvent>((evt) => { nameDropdown.choices = GetList(graphView); });
            nameDropdown.RegisterValueChangedCallback(evt => { value.Name = evt.newValue; BindProperty(); });
            Add(nameDropdown);
        }
        public sealed override PieceID value
        {
            get => base.value;
            set
            {
                if (value != null)
                {
                    base.value = value.Clone() as PieceID;
                    if (graphView != null)
                    {
                        BindProperty();
                        UpdateValueField();
                    }
                }
                else
                {
                    base.value = new PieceID();
                }
            }
        }
    }
}