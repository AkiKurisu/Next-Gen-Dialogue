using System.Collections.Generic;
using Cysharp.Threading.Tasks;
namespace NextGenDialogue
{
    public class DefaultOptionResolver : IOptionResolver
    {
        private DialogueSystem _system;
        
        public IReadOnlyList<Option> DialogueOptions { get; private set; }
        
        protected ObjectContainer ObjectContainer { get; } = new();
        
        public void Inject(IReadOnlyList<Option> options, DialogueSystem system)
        {
            DialogueOptions = options;
            _system = system;
        }
        
        public UniTask ClickOption(Option option)
        {
            CallBackModule.InvokeCallBack(option);
            if (string.IsNullOrEmpty(option.TargetID))
            {
                //Exit Dialogue
                _system.EndDialogue(false);
            }
            else
            {
                _system.PlayDialoguePiece(option.TargetID);
            }

            return UniTask.CompletedTask;
        }
        
        public async UniTask EnterOption()
        {
            foreach (var option in DialogueOptions)
            {
                ObjectContainer.Register<IContentModule>(option);
                await option.ProcessModules(ObjectContainer);
                await OnOptionResolve(option);
            }
        }
        
        protected virtual UniTask OnOptionResolve(Option option)
        {
            return UniTask.CompletedTask;
        }
    }
}
