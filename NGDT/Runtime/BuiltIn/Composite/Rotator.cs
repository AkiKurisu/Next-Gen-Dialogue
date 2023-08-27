namespace Kurisu.NGDT
{
    [AkiInfo("Composite : Rotator, update child nodes in order, each Update will only update the current node" +
    ", after the node finishes running, the next Update will continue to update the next node")]
    public class Rotator : Composite
    {
        private int targetIndex;
        protected override Status OnUpdate()
        {
            var status = Children[targetIndex].Update();
            SetNext();
            return status;
        }

        private void SetNext()
        {
            targetIndex++;
            if (targetIndex >= Children.Count)
            {
                targetIndex = 0;
            }
        }
    }
}