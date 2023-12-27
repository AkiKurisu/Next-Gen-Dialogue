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
        private static Piece GetFirstPiece()
        {
            var piece = Piece.CreatePiece();
            piece.Content = "This is the first dialogue piece";
            piece.PieceID = "01";
            piece.AddOption(new Option()
            {
                Content = "Jump to Next",
                TargetID = "02"
            });
            return piece;
        }
        private static Piece GetSecondPiece()
        {
            var piece = Piece.CreatePiece();
            piece.Content = "This is the second dialogue piece";
            piece.PieceID = "02";
            piece.AddOption(GetFirstOption());
            piece.AddOption(GetSecondOption());
            return piece;
        }
        private static Option GetFirstOption()
        {
            var callBackOption = Option.CreateOption();
            //Add CallBack Module
            callBackOption.AddModule(new CallBackModule(() => Debug.Log("Hello World !")));
            callBackOption.Content = "Log";
            return callBackOption;
        }
        private static Option GetSecondOption()
        {
            var option = Option.CreateOption();
            option.Content = "Back To First";
            option.TargetID = "01";
            return option;
        }
    }
}