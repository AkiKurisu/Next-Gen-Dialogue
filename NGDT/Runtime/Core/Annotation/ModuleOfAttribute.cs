using System;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Constraint the container nodes this module belongs to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ModuleOfAttribute : Attribute
    {
        private readonly Type containerType;
        public Type ContainerType => containerType;
        public ModuleOfAttribute(Type containerType)
        {
            this.containerType = containerType;
        }
    }
}
