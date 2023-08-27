using System.Collections;
using UnityEngine;
namespace Kurisu.NGDS.Example
{
    public class CodeDialogueBuilder : MonoBehaviour
    {
        private DialogueGenerator generator;
        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            PlayDialogue();
        }
        private void PlayDialogue()
        {
            var dialogueSystem = IOCContainer.Resolve<IDialogueSystem>();
            generator = new();
            //First Piece
            generator.AddPiece(GetFirstPiece());
            //Second Piece
            generator.AddPiece(GetSecondPiece());
            dialogueSystem.StartDialogue(generator);
        }
        private static DialoguePiece GetFirstPiece()
        {
            var piece = DialoguePiece.CreatePiece();
            piece.Content = "This is the first dialogue piece";
            piece.PieceID = "01";
            piece.AddOption(new DialogueOption()
            {
                Content = "Jump to Next",
                TargetID = "02"
            });
            return piece;
        }
        private static DialoguePiece GetSecondPiece()
        {
            var piece = DialoguePiece.CreatePiece();
            piece.Content = "This is the second dialogue piece";
            piece.PieceID = "02";
            piece.AddOption(GetFirstOption());
            piece.AddOption(GetSecondOption());
            return piece;
        }
        private static DialogueOption GetFirstOption()
        {
            var callBackOption = DialogueOption.CreateOption();
            //Add CallBack Module
            callBackOption.AddModule(new CallBackModule(() => Debug.Log("Hello World !")));
            callBackOption.Content = "Log";
            return callBackOption;
        }
        private static DialogueOption GetSecondOption()
        {
            var option = DialogueOption.CreateOption();
            option.Content = "Back To First";
            option.TargetID = "01";
            return option;
        }
    }
}