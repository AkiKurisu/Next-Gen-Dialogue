using System;
namespace Ceres.Annotations
{
    /// <summary>
    /// Wrap object to use legacy IMGUI property field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class WrapFieldAttribute : Attribute
    {

    }
}
