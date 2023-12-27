using System;
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
    }
}
