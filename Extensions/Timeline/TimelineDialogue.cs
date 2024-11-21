using System;
using Kurisu.Framework;
using Kurisu.NGDS;
using UnityEngine;
using UnityEngine.Playables;
namespace Kurisu.NGDT.Timeline
{
    public class TimelineDialogue : MonoBehaviour, INotificationReceiver
    {
        [Serializable]
        private class DialogueReceiver
        {
            public string dialogueName;
            public NextGenDialogueComponent dialogueTree;
        }
        
        [SerializeField]
        private DialogueReceiver[] receivers;
        
        private NextGenDialogueComponent _dialogueTree;
        
        private PlayableDirector _director;
        
        private IDialogueSystem _dialogueSystem;
        private void Awake()
        {
            _director = GetComponent<PlayableDirector>();
        }
        private void Start()
        {
            _dialogueSystem = ContainerSubsystem.Get().Resolve<IDialogueSystem>();
        }
        private void OnDestroy()
        {
            _dialogueSystem.OnDialogueOver -= ContinueDialogue;
        }
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is not DialogueSignal dialogueSignal) return;
            
            if (dialogueSignal.dialogueAsset != null)
            {
                if (_dialogueTree == null) _dialogueTree = gameObject.AddComponent<NextGenDialogueComponent>();
                _dialogueTree.ExternalData = dialogueSignal.dialogueAsset;
                _dialogueTree.GetDialogueGraph().PlayDialogue(_dialogueTree.gameObject);
            }
            else
            {
                if (TryFindReceiver(dialogueSignal.dialogueName, out var receiver))
                {
                    receiver.dialogueTree.GetDialogueGraph().PlayDialogue(receiver.dialogueTree.gameObject);
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
