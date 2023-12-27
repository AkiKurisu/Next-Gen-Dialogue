using System.Collections.Generic;
namespace Kurisu.NGDS
{
    internal class NodePoolManager
    {
        private static NodePoolManager instance;
        public static NodePoolManager Instance => instance ?? new NodePoolManager();
        public NodePoolManager()
        {
            instance = this;
        }
        public Dictionary<string, NodePoolData> nodePoolDic = new();
        public T GetNode<T>() where T : Node, new()
        {
            T obj;
            if (CheckNodeCache<T>())
            {
                string name = typeof(T).Name;
                obj = (T)nodePoolDic[name].GetNode();
                return obj;
            }
            else
            {
                return new T();
            }
        }
        public void PushNode(Node obj)
        {
            string name = obj.GetType().Name;
            if (nodePoolDic.ContainsKey(name))
            {
                nodePoolDic[name].PushNode(obj);
            }
            else
            {
                nodePoolDic.Add(name, new NodePoolData(obj));
            }
        }
        private bool CheckNodeCache<T>()
        {
            string name = typeof(T).Name;
            return nodePoolDic.ContainsKey(name) && nodePoolDic[name].poolQueue.Count > 0;
        }
    }
    public class NodePoolData
    {
        public NodePoolData(Node obj)
        {
            PushNode(obj);
        }
        public Queue<Node> poolQueue = new();
        public void PushNode(Node obj)
        {
            poolQueue.Enqueue(obj);
        }
        public Node GetNode()
        {
            return poolQueue.Dequeue();
        }
    }
    public static class NodePoolExtension
    {
        public static void NodePushPool(this Node obj)
        {
            foreach (var childNode in obj.ChildNodes()) childNode.NodePushPool();
            NodePoolManager.Instance.PushNode(obj);
        }
    }
}
