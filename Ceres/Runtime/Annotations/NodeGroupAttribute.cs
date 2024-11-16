using System;
namespace Ceres.Annotations
{
    /// <summary>
    /// Nodes are categorized in the editor dropdown menu, and can be sub-categorized with the '/' symbol
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class NodeGroupAttribute : Attribute
    {
        public string Group
        {
            get
            {
                return mGroup;
            }
        }

        private readonly string mGroup;
        public NodeGroupAttribute(string group)
        {
            mGroup = group;
        }
    }
}