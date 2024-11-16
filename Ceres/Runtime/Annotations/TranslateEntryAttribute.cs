using System;
using UnityEngine;
namespace Ceres.Annotations
{
    /// <summary>
    /// Notify field can be translated by editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TranslateEntryAttribute : PropertyAttribute { }
}
