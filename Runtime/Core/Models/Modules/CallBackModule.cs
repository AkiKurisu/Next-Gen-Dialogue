using System;
using UnityEngine.Pool;

namespace NextGenDialogue
{
    public class CallBackModule : IDialogueModule
    {
        private readonly Action _callBack;
        
        public CallBackModule(Action callBack)
        {
            _callBack = callBack;
        }
        
        public void InvokeCallBack()
        {
            _callBack?.Invoke();
        }
        
        public static void InvokeCallBack(Node node)
        {
            var moduleCache = ListPool<CallBackModule>.Get();
            try
            {
                node.CollectModules(moduleCache);
                foreach (var callbackModule in moduleCache)
                {
                    callbackModule.InvokeCallBack();
                }
            }
            finally
            {
                ListPool<CallBackModule>.Release(moduleCache);
            }
        }
    }
}
