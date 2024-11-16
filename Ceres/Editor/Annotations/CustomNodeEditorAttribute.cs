using System;
namespace Ceres.Editor
{
    /// <summary>
    /// Tells an Node class which run-time node type it's an editor for.
    /// </summary>
    public sealed class CustomNodeEditorAttribute : Attribute
    {
        public Type InspectedType { get; }
        public bool EditorForChildClasses { get; }
        public CustomNodeEditorAttribute(Type inspectedType)
        {
            InspectedType = inspectedType;
            EditorForChildClasses = false;
        }
        public CustomNodeEditorAttribute(Type inspectedType, bool editorForChildClasses)
        {
            InspectedType = inspectedType;
            EditorForChildClasses = editorForChildClasses;
        }
    }
}