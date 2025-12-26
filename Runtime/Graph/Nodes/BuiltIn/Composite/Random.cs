using System;
using Ceres.Annotations;
using URandom = UnityEngine.Random;

namespace NextGenDialogue.Graph
{
    [Obsolete("Random is no longer used, use Flow instead.")]
    [NodeInfo("Composite: Random, random update a child and reselect the next node")]
    public class Random : CompositeNode
    {
        protected override Status OnUpdate()
        {
            var result = URandom.Range(0, Children.Count);
            var target = Children[result];
            return target.Update();
        }
    }
}