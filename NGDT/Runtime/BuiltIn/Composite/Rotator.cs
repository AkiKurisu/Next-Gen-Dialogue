using Ceres.Annotations;
namespace Kurisu.NGDT
{
    [NodeInfo("Composite : Rotator, update child nodes in order, each Update will only update the current node" +
    ", after the node finishes running, the next Update will continue to update the next node")]
    public class Rotator : CompositeNode
    {
        private int _targetIndex;
        
        protected override Status OnUpdate()
        {
            var status = Children[_targetIndex].Update();
            SetNext();
            return status;
        }

        private void SetNext()
        {
            _targetIndex++;
            if (_targetIndex >= Children.Count)
            {
                _targetIndex = 0;
            }
        }
    }
}