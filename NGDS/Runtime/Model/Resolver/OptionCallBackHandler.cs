using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public class OptionCallBackHandler
    {
        private readonly List<CallBackModule> moduleCache = new();
        public void Handle(Option option)
        {
            moduleCache.Clear();
            option.CollectModules(moduleCache);
            for (int i = 0; i < moduleCache.Count; i++)
            {
                moduleCache[i].InvokeCallBack();
            }
        }
    }
}