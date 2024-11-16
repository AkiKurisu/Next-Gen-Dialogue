using System;
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
            public NextGenDialogueTree dialogueTree;
        }
        [SerializeField]
        private DialogueReceiver[] receivers;
        private NextGenDialogueTree dialogueTree;
        private PlayableDirector director;
        private IDialogueSystem dialogueSystem;
        private void Awake()
        {
            director = GetComponent<PlayableDirector>();
        }
        private void Start()
        {
            dialogueSystem = IOCContainer.Resolve<IDialogueSystem>();
        }
        private void OnDestroy()
        {
            dialogueSystem.OnDialogueOver -= ContinueDialogue;
        }
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is DialogueSignal dialogueSignal)
            {
                if (dialogueSignal.dialogueAsset != null)
                {
                    if (dialogueTree == null) dialogueTree = gameObject.AddComponent<NextGenDialogueTree>();
                    dialogueTree.ExternalData = dialogueSignal.dialogueAsset;
                    dialogueTree.GetDialogueGraph().PlayDialogue(dialogueTree.gameObject);
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
                    director.playableGraph.GetRootPlayable(0).SetSpeed(0d);
                    dialogueSystem.OnDialogueOver += ContinueDialogue;
                }
            }
        }
        private void ContinueDialogue()
        {
            dialogueSystem.OnDialogueOver -= ContinueDialogue;
            director.playableGraph.GetRootPlayable(0).SetSpeed(1d);
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
