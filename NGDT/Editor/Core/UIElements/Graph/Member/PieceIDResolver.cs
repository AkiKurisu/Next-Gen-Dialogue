using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ceres;
using Ceres.Editor;
using Ceres.Editor.Graph;
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
        private CeresGraphView _graphView;
        
        private DropdownField _nameDropdown;
        
        internal SharedVariable BindVariable;
        
        private readonly bool _isReferenced;
        public PieceIDField(string label, bool isReferenced) : base(label, null)
        {
            AddToClassList("SharedVariableField");
            _isReferenced = isReferenced;
            if (!isReferenced) AddToClassList("PieceIDField");
        }
        public void BindGraph(CeresGraphView graph)
        {
            this._graphView = graph;
            this._graphView.Blackboard.RegisterCallback<VariableChangeEvent>(evt =>
            {
                if (evt.ChangeType != VariableChangeType.NameChange) return;
                if (evt.Variable != BindVariable) return;
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
            if (_isReferenced)
            {
                _nameDropdown.value = variable.Name;
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
            BindVariable = _graphView.SharedVariables.FirstOrDefault(x => x.GetType() == typeof(PieceID) && x.Name.Equals(value.Name));
        }
        private void UpdateValueField()
        {
            if (_isReferenced)
            {
                if (_nameDropdown == null) AddDropDown();
                else _nameDropdown.value = value.Name;
            }
            else
            {
                label = value.Name;
            }
        }
        private void AddDropDown()
        {
            var list = GetList(_graphView);
            value.Name = value.Name ?? string.Empty;
            int index = list.IndexOf(value.Name);
            _nameDropdown = new DropdownField("Piece ID", list, index);
            _nameDropdown.RegisterCallback<MouseEnterEvent>((evt) => { _nameDropdown.choices = GetList(_graphView); });
            _nameDropdown.RegisterValueChangedCallback(evt => { value.Name = evt.newValue; BindProperty(); });
            Add(_nameDropdown);
        }
        public sealed override PieceID value
        {
            get => base.value;
            set
            {
                if (value != null)
                {
                    base.value = value.Clone() as PieceID;
                    if (_graphView != null)
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