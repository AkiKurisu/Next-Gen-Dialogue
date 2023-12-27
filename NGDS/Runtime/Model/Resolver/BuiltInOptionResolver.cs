using System.Collections;
using System.Collections.Generic;
namespace Kurisu.NGDS
{
    public class BuiltInOptionResolver : IOptionResolver
    {
        private IDialogueSystem system;
        public IReadOnlyList<Option> DialogueOptions { get; private set; }
        private readonly OptionCallBackHandler callBackHandler = new();
        protected ObjectContainer ObjectContainer { get; } = new();
        public void Inject(IReadOnlyList<Option> options, IDialogueSystem system)
        {
            DialogueOptions = options;
            this.system = system;
        }
        public IEnumerator ClickOption(Option option)
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
                    if (option.Modules[i] is IProcessable injectable)
                        yield return injectable.Process(ObjectContainer);
                    yield return OnOptionResolve(option);
                }
            }
        }
        protected virtual IEnumerator OnOptionResolve(Option option)
        {
            yield break;
        }
    }
}
