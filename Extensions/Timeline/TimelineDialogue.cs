using System;
using Kurisu.NGDS;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
namespace Kurisu.NGDT.Timeline
{
    public class TimelineDialogue : MonoBehaviour, INotificationReceiver
    {
        [Serializable]
        private class DialogueReceiver
        {
            public string dialogueName;
            [FormerlySerializedAs("dialogueTree")] public NextGenDialogueGraphComponent dialogueGraphTree;
        }
        
        [SerializeField]
        private DialogueReceiver[] receivers;
        
        private NextGenDialogueGraphComponent _dialogueGraphTree;
        
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
        
        private void OnDestroy()
        {
            _dialogueSystem.OnDialogueOver -= ContinueDialogue;
        }
        
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not DialogueSignal dialogueSignal) return;
            
            if (dialogueSignal.dialogueGraphAsset != null)
            {
                if (_dialogueGraphTree == null) _dialogueGraphTree = gameObject.AddComponent<NextGenDialogueGraphComponent>();
                _dialogueGraphTree.ExternalData = dialogueSignal.dialogueGraphAsset;
                _dialogueGraphTree.GetDialogueGraph().PlayDialogue(_dialogueGraphTree.gameObject);
            }
            else
            {
                if (TryFindReceiver(dialogueSignal.dialogueName, out var receiver))
                {
                    receiver.dialogueGraphTree.GetDialogueGraph().PlayDialogue(receiver.dialogueGraphTree.gameObject);
                }
                else
                {
                    Debug.LogError($"Can not find dialogue receiver for {dialogueSignal.dialogueName}");
                }
            }
            if (dialogueSignal.pausePlayable)
            {
                _director.playableGraph.GetRootPlayable(0).SetSpeed(0d);
                _dialogueSystem.OnDialogueOver += ContinueDialogue;
            }
        }
        
        private void ContinueDialogue()
        {
            _dialogueSystem.OnDialogueOver -= ContinueDialogue;
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
