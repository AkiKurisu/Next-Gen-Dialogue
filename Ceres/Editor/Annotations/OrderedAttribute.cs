using System;
namespace Ceres.Editor
{
    /// <summary>
    /// Give priority for resolver (node or field)
    /// </summary>
    public sealed class OrderedAttribute : Attribute
    {
        public int Order = 100;
    }
}