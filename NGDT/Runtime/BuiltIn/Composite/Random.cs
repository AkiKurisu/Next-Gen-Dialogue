namespace Kurisu.NGDT
{
    [AkiInfo("Composite : Random, random update a child and reselect the next node")]
    public class Random : Composite
    {
        protected override Status OnUpdate()
        {
            var result = UnityEngine.Random.Range(0, Children.Count);
            var target = Children[result];
            return target.Update();
        }
    }
}