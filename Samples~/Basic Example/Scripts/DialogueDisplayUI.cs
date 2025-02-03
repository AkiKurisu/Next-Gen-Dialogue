using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Cysharp.Threading.Tasks;
using R3;
namespace NextGenDialogue.Example
{
    public class DialogueDisplayUI : MonoBehaviour
    {
        [SerializeField]
        private Text mainText;
        
        [SerializeField]
        private Transform optionPanel;
        
        private readonly List<OptionUI> optionSlots = new();
        
        [SerializeField]
        private OptionUI optionPrefab;
        
        private DialogueSystem _dialogueSystem;
        
        [SerializeField]
        private float delayForWord = 0.05f;
        
        private readonly StringBuilder _stringBuilder = new();
        
        private void Start()
        {
            _dialogueSystem = DialogueSystem.Get();
            _dialogueSystem.OnDialogueOver.Subscribe(DialogueOverHandler);
            _dialogueSystem.OnPiecePlay.Subscribe(PlayDialoguePiece);
            _dialogueSystem.OnOptionCreate.Subscribe(CreateOption);
        }
        
        private void DialogueOverHandler(Unit _)
        {
            StopCoroutine(nameof(WaitOver));
            StartCoroutine(nameof(WaitOver));
        }
        
        private IEnumerator WaitOver()
        {
            yield return new WaitForSeconds(1f);
            CleanUp();
        }
        
        private void PlayDialoguePiece(IPieceResolver resolver)
        {
            StopCoroutine(nameof(WaitOver));
            CleanUp();
            PlayText(resolver.DialoguePiece.Contents, () => resolver.ExitPiece().Forget()).Forget();
        }
        
        private async UniTask PlayText(string[] contents, System.Action callBack)
        {
            foreach (var text in contents)
            {
                int count = text.Length;
                mainText.text = string.Empty;
                _stringBuilder.Clear();
                for (int i = 0; i < count; i++)
                {
                    _stringBuilder.Append(text[i]);
                    mainText.text = _stringBuilder.ToString();
                    await UniTask.WaitForSeconds(delayForWord);
                }
            }
            callBack?.Invoke();
        }
        
        private void CreateOption(IOptionResolver resolver)
        {
            foreach (var option in resolver.DialogueOptions)
            {
                var optionSlot = Instantiate(optionPrefab, optionPanel);
                optionSlots.Add(optionSlot);
                optionSlot.UpdateOption(option, selectOption => ClickOptionAsync(resolver, selectOption).Forget());
            }
        }
        
        private async UniTask ClickOptionAsync(IOptionResolver resolver, Option opt)
        {
            await resolver.ClickOption(opt);
            CleanUp();
        }
        
        private void CleanUp()
        {
            mainText.text = string.Empty;
            foreach (var slot in optionSlots) Destroy(slot.gameObject);
            optionSlots.Clear();
        }
    }
}
