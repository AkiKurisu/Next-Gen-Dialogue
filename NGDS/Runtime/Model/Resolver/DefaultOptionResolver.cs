using System.Collections;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public class DefaultOptionResolver : IOptionResolver
    {
        private IDialogueSystem system;
        public IReadOnlyList<Option> DialogueOptions { get; private set; }
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(IReadOnlyList<Option> options, IDialogueSystem system)
        {
            DialogueOptions = options;
            this.system = system;
        }
        public IEnumerator ClickOption(Option option)
        {
            CallBackModule.InvokeCallBack(option);
            if (string.IsNullOrEmpty(option.TargetID))
            {
                //Exit Dialogue
                system.EndDialogue();
            }
            else
            {
                system.PlayDialoguePiece(option.TargetID);
            }
            yield return null;
        }
        public IEnumerator EnterOption()
        {
            foreach (var option in DialogueOptions)
            {
                ObjectContainer.Register<IContentModule>(option);
                yield return option.ProcessModules(ObjectContainer);
                yield return OnOptionResolve(option);
            }
        }
        protected virtual IEnumerator OnOptionResolve(Option option)
        {
            yield break;
        }
    }
}
