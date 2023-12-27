using System;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Wrap object to use legacy IMGUI property field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class WrapFieldAttribute : Attribute
    {

    }
}
