using System;
using UnityEngine;
namespace NextGenDialogue.Graph
{
    /// <summary>
    /// Notify field can be translated by editor
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TranslateEntryAttribute : PropertyAttribute { }
}
