using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Composite : Weighted random, randomly selected according to the weight")]
    public class WeightedRandom : Composite
    {
        [SerializeField, Tooltip("Node weight list, when the length of the list is greater than the number of child nodes" +
        ", the excess part will not be included in the weight")]
        private List<float> weights = new();
        protected override Status OnUpdate()
        {
            var result = GetNext();
            var target = Children[result];
            return target.Update();
        }

        private int GetNext()
        {
            float total = 0;
            int count = Mathf.Min(weights.Count, Children.Count);
            for (int i = 0; i < count; i++)
            {
                total += weights[i];
            }
            float random = UnityEngine.Random.Range(0, total);

            for (int i = 0; i < count; i++)
            {
                if (random < weights[i]) return i;
                random -= weights[i];
            }
            return 0;
        }
    }
}