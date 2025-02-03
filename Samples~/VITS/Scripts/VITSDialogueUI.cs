using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Kurisu.NGDS;
using Kurisu.NGDS.VITS;
using UnityEngine;
using UnityEngine.UI;
namespace Kurisu.NGDT.VITS.Example
{
    public class VITSDialogueUI : MonoBehaviour
    {
        [SerializeField]
        private Text mainText;
        
        [SerializeField]
        private Transform optionPanel;
        
        private readonly List<VITSOptionUI> _optionSlots = new();
        
        [SerializeField]
        private VITSOptionUI optionPrefab;
        
        private DialogueSystem _dialogueSystem;
        
        [SerializeField]
        private AudioSource audioSource;
        
        [SerializeField, Header("FallBack")]
        private float delayForWord = 0.05f;
        
        private void Start()
        {
            _dialogueSystem = DialogueSystem.Get();
            _dialogueSystem.OnDialogueOver += DialogueOverHandler;
            _dialogueSystem.OnPiecePlay += PlayDialoguePiece;
            _dialogueSystem.OnOptionCreate += CreateOption;
        }
        
        private void DialogueOverHandler()
        {
            StopCoroutine(nameof(WaitOver));
            StartCoroutine(nameof(WaitOver));
        }
        
        private IEnumerator WaitOver()
        {
            yield return new WaitForSeconds(1f);
            CleanUp();
        }
        
        private void OnDestroy()
        {
            _dialogueSystem.OnDialogueOver -= DialogueOverHandler;
            _dialogueSystem.OnPiecePlay -= PlayDialoguePiece;
            _dialogueSystem.OnOptionCreate -= CreateOption;
        }
        
        private void PlayDialoguePiece(IPieceResolver resolver)
        {
            StopCoroutine(nameof(WaitOver));
            CleanUp();
            StartCoroutine(PlayText(resolver.DialoguePiece.Contents, ((VITSPieceResolver)resolver).AudioClips, () => resolver.ExitPiece().Forget()));
        }
        
        private readonly StringBuilder _stringBuilder = new();
        
        private IEnumerator PlayText(string[] contents, AudioClip[] audioClips, System.Action callBack)
        {
            for (int i = 0; i < contents.Length; ++i)
            {
                var text = contents[i];
                var clip = audioClips[i];
                WaitForSeconds seconds;
                if (clip)
                {
                    audioSource.clip = clip;
                    audioSource.Play();
                    seconds = new WaitForSeconds(clip.length / text.Length);
                }
                else
                {
                    seconds = new WaitForSeconds(delayForWord);
                }
                int count = text.Length;
                mainText.text = string.Empty;
                _stringBuilder.Clear();
                for (int n = 0; n < count; n++)
                {
                    _stringBuilder.Append(text[n]);
                    mainText.text = _stringBuilder.ToString();
                    yield return seconds;
                }
            }
            callBack?.Invoke();
        }
        
        private void CreateOption(IOptionResolver resolver)
        {
            foreach (var option in resolver.DialogueOptions)
            {
                VITSOptionUI optionSlot = GetOption();
                _optionSlots.Add(optionSlot);
                optionSlot.UpdateOption(option, (opt) => ClickOptionCoroutine(resolver, opt).Forget());
            }
        }
        
        private async UniTask ClickOptionCoroutine(IOptionResolver resolver, Option opt)
        {
            await resolver.ClickOption(opt);
            CleanUp();
        }
        
        private void CleanUp()
        {
            mainText.text = string.Empty;
            foreach (var slot in _optionSlots) Destroy(slot.gameObject);
            _optionSlots.Clear();
        }
        
        private VITSOptionUI GetOption()
        {
            return Instantiate(optionPrefab, optionPanel);
        }
    }
}
