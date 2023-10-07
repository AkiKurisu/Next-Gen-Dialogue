using System.Collections;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public class OptionCallBackHandler
    {
        private readonly List<CallBackModule> moduleCache = new();
        public void Handle(DialogueOption option)
        {
            moduleCache.Clear();
            option.CollectModules(moduleCache);
            for (int i = 0; i < moduleCache.Count; i++)
            {
                moduleCache[i].InvokeCallBack();
            }
        }
    }
    public class BuiltInOptionResolver : IOptionResolver
    {
        private IDialogueSystem system;
        public IReadOnlyList<DialogueOption> DialogueOptions { get; private set; }
        private readonly OptionCallBackHandler callBackHandler = new();
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(IReadOnlyList<DialogueOption> options, IDialogueSystem system)
        {
            DialogueOptions = options;
            this.system = system;
        }
        public IEnumerator ClickOption(DialogueOption option)
        {
            if (string.IsNullOrEmpty(option.TargetID))
            {
                //Exit Dialogue
                system.EndDialogue();
            }
            else
            {
                system.PlayDialoguePiece(option.TargetID);
            }
            //Handle CallBack Module
            callBackHandler.Handle(option);
            yield return null;
        }
        public IEnumerator EnterOption()
        {
            foreach (var option in DialogueOptions)
            {
                ObjectContainer.Register<IContent>(option);
                for (int i = 0; i < option.Modules.Count; i++)
                {
                    if (option.Modules[i] is IInjectable injectable)
                        yield return injectable.Inject(ObjectContainer);
                    yield return OnOptionResolve(option);
                }
            }
        }
        protected virtual IEnumerator OnOptionResolve(DialogueOption option)
        {
            yield return null;
        }
    }
}
