using System;
namespace Ceres.Annotations
{
    /// <summary>
    /// Use language code in string field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class LanguageCodeAttribute : Attribute
    {

    }
}
