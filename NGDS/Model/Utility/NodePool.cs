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
        public T GetNode<T>() where T : DialogueNode, new()
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
        public void PushNode(DialogueNode obj)
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
        public NodePoolData(DialogueNode obj)
        {
            PushNode(obj);
        }
        public Queue<DialogueNode> poolQueue = new();
        public void PushNode(DialogueNode obj)
        {
            poolQueue.Enqueue(obj);
        }
        public DialogueNode GetNode()
        {
            return poolQueue.Dequeue();
        }
    }
    public static class NodePoolExtension
    {
        public static void NodePushPool(this DialogueNode obj)
        {
            foreach (var childNode in obj.ChildNodes()) childNode.NodePushPool();
            NodePoolManager.Instance.PushNode(obj);
        }
    }
}
