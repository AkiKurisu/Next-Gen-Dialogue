using System;
namespace Kurisu.NGDT
{
    /// <summary>
    /// Use lanaguage code in string field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class LanguageCodeAttribute : Attribute
    {

    }
}
