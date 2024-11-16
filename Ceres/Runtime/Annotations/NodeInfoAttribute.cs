using System;
namespace Ceres.Annotations
{
    /// <summary>
    /// Describe node behavior in the editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class NodeInfoAttribute : Attribute
    {
        public string Description
        {
            get
            {
                return mDescription;
            }
        }

        private readonly string mDescription;
        public NodeInfoAttribute(string description)
        {
            mDescription = description;
        }
    }
}