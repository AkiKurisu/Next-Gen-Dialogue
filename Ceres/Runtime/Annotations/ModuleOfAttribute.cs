using System;
namespace Ceres.Annotations
{
    /// <summary>
    /// Constraint the container nodes this module belongs to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ModuleOfAttribute : Attribute
    {
        public Type ContainerType { get; }
        /// <summary>
        /// Allow container contains multi instance of this module
        /// </summary>
        /// <value></value>
        public bool AllowMulti { get; }
        public ModuleOfAttribute(Type containerType, bool allowMulti = false)
        {
            ContainerType = containerType;
            AllowMulti = allowMulti;
        }
    }
}
