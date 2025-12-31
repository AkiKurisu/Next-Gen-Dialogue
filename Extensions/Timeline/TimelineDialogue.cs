using System;
using NextGenDialogue.Graph;
using R3;
using UnityEngine;
using UnityEngine.Playables;

namespace NextGenDialogue.Timeline
{
    public class TimelineDialogue : MonoBehaviour, INotificationReceiver
    {
        [Serializable]
        private class DialogueReceiver
        {
            public string dialogueName;
            
            public NextGenDialogueComponent dialogueComponent;
        }
        
        [SerializeField]
        private DialogueReceiver[] receivers;
        
        private NextGenDialogueComponent _dialogueComponent;
        
        private PlayableDirector _director;
        
        private DialogueSystem _dialogueSystem;
        
        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }
        
        private void Start()
        {
            _dialogueSystem = DialogueSystem.Get();
        }
        
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not DialogueSignal dialogueSignal) return;
            
            if (dialogueSignal.dialogueGraphAsset != null)
            {
                if (_dialogueComponent == null) _dialogueComponent = gameObject.AddComponent<NextGenDialogueComponent>();
                _dialogueComponent.PlayDialogue(dialogueSignal.dialogueGraphAsset);
            }
            else
            {
                if (TryFindReceiver(dialogueSignal.dialogueName, out var receiver))
                {
                    receiver.dialogueComponent.PlayDialogue();
                }
                else
                {
                    Debug.LogError($"Can not find dialogue receiver for {dialogueSignal.dialogueName}");
                }
            }
            if (dialogueSignal.pausePlayable)
            {
                _director.playableGraph.GetRootPlayable(0).SetSpeed(0d);
                _dialogueSystem.OnDialogueOver.Take(1).Subscribe(ContinueDialogue).AddTo(this);
            }
        }
        
        private void ContinueDialogue(Unit _)
        {
            _director.playableGraph.GetRootPlayable(0).SetSpeed(1d);
        }
        
        private bool TryFindReceiver(string dialogueName, out DialogueReceiver dialogueReceiver)
        {
            foreach (var receiver in receivers)
            {
                if (receiver.dialogueName == dialogueName)
                {
                    dialogueReceiver = receiver;
                    return true;
                }
            }
            dialogueReceiver = null;
            return false;
        }
    }
}
