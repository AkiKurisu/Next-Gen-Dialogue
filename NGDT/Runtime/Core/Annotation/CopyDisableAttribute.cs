using System;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Disable field copying within the editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CopyDisableAttribute : Attribute
    {

    }
}
