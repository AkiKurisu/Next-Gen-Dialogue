namespace Kurisu.NGDS
{
    public static class NodePoolExtension
    {
        public static void DisposeRecursively(this Node obj)
        {
            foreach (var childNode in obj.ChildNodes())
            {
                childNode.DisposeRecursively();
            }
            obj.Dispose();
        }
    }
}
