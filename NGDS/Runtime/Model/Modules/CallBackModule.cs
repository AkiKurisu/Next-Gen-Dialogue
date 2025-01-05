using System;
using UnityEngine.Pool;
namespace Kurisu.NGDS
{
    public readonly struct CallBackModule : IDialogueModule
    {
        private readonly Action callBack;
        public CallBackModule(Action callBack)
        {
            this.callBack = callBack;
        }
        public void InvokeCallBack()
        {
            callBack?.Invoke();
        }
        public static void InvokeCallBack(Node node)
        {
            var moduleCache = ListPool<CallBackModule>.Get();
            try
            {
                node.CollectModules(moduleCache);
                for (int i = 0; i < moduleCache.Count; i++)
                {
                    moduleCache[i].InvokeCallBack();
                }
            }
            finally
            {
                ListPool<CallBackModule>.Release(moduleCache);
            }
        }
    }
}
