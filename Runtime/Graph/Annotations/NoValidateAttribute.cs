using System;
namespace NextGenDialogue.Graph
{
    /// <summary>
    /// Skip Composite check port legitimacy, allow the port not to connect to the node after use
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NoValidateAttribute : Attribute
    {

    }
}
