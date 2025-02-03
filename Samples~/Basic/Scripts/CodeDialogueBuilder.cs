using UnityEngine;
namespace Kurisu.NGDS.Example
{
    public class CodeDialogueBuilder : MonoBehaviour
    {
        private DialogueBuilder _builder;
        
        private void Start()
        {
            PlayDialogue();
        }
        
        private void PlayDialogue()
        {
            var dialogueSystem = DialogueSystem.Get();
            _builder = new DialogueBuilder();
            // First Piece
            _builder.AddPiece(GetFirstPiece());
            // Second Piece
            _builder.AddPiece(GetSecondPiece());
            dialogueSystem.StartDialogue(_builder);
        }
        
        private static Piece GetFirstPiece()
        {
            var piece = Piece.GetPooled();
            piece.Contents = new string[1] { "This is the first dialogue piece" };
            piece.ID = "01";
            piece.AddOption(new Option()
            {
                Content = "Jump to Next",
                TargetID = "02"
            });
            return piece;
        }
        
        private static Piece GetSecondPiece()
        {
            var piece = Piece.GetPooled();
            piece.Contents = new string[1] { "This is the second dialogue piece" };
            piece.ID = "02";
            piece.AddOption(GetFirstOption());
            piece.AddOption(GetSecondOption());
            return piece;
        }
        
        private static Option GetFirstOption()
        {
            var callBackOption = Option.GetPooled();
            // Add CallBack Module
            callBackOption.AddModule(new CallBackModule(() => Debug.Log("Hello World!")));
            callBackOption.Content = "Log";
            return callBackOption;
        }
        
        private static Option GetSecondOption()
        {
            var option = Option.GetPooled();
            option.Content = "Back To First";
            option.TargetID = "01";
            return option;
        }
    }
}