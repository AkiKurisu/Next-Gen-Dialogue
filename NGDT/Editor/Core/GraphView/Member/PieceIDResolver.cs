using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
namespace Kurisu.NGDT.Editor
{
    public class PieceIDResolver : FieldResolver<PieceIDField, PieceID>
    {
        public PieceIDResolver(FieldInfo fieldInfo) : base(fieldInfo)
        {
        }
        protected override void SetTree(IDialogueTreeView ownerTreeView)
        {
            editorField.InitField(ownerTreeView);
        }
        private PieceIDField editorField;
        protected override PieceIDField CreateEditorField(FieldInfo fieldInfo)
        {
            bool isReferenced = fieldInfo.GetCustomAttribute<ReferencePieceIDAttribute>() != null;
            editorField = new PieceIDField(fieldInfo.Name, null, isReferenced);
            return editorField;
        }
        public static bool IsAcceptable(Type infoType, FieldInfo _) => infoType == typeof(PieceID);

    }
    public class PieceIDField : BaseField<PieceID>
    {
        private IDialogueTreeView treeView;
        private DropdownField nameDropdown;
        private SharedVariable bindExposedProperty;
        private readonly bool isReferenced;
        public PieceIDField(string label, IDialogueTreeView treeView, bool isReferenced) : base(label, null)
        {
            AddToClassList("SharedVariableField");
            this.treeView = treeView;
            this.isReferenced = isReferenced;
            if (!isReferenced) AddToClassList("PieceIDField");
        }
        internal void InitField(IDialogueTreeView treeView)
        {
            this.treeView = treeView;
            treeView.BlackBoard.View.RegisterCallback<VariableChangeEvent>(evt =>
            {
                if (evt.ChangeType != VariableChangeType.NameChange) return;
                if (evt.Variable != bindExposedProperty) return;
                UpdateID(evt.Variable);
            });
            BindProperty();
            UpdateValueField();
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
        private static List<string> GetList(IDialogueTreeView treeView)
        {
            return treeView.SharedVariables
            .Where(x => x.GetType() == typeof(PieceID))
            .Select(v => v.Name)
            .ToList();
        }
        private void BindProperty()
        {
            bindExposedProperty = treeView.SharedVariables.Where(x => x.GetType() == typeof(PieceID) && x.Name.Equals(value.Name)).FirstOrDefault();
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
            var list = GetList(treeView);
            value.Name = value.Name ?? string.Empty;
            int index = list.IndexOf(value.Name);
            nameDropdown = new DropdownField("Piece ID", list, index);
            nameDropdown.RegisterCallback<MouseEnterEvent>((evt) => { nameDropdown.choices = GetList(treeView); });
            nameDropdown.RegisterValueChangedCallback(evt => { value.Name = evt.newValue; BindProperty(); });
            Add(nameDropdown);
        }
        public sealed override PieceID value
        {
            get => base.value; set
            {
                if (value != null)
                {
                    base.value = value.Clone() as PieceID;
                    BindProperty();
                    UpdateValueField();
                }
                else
                {
                    base.value = new PieceID();
                }
            }
        }
    }
}