using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDS
{
    public class Dialogue : DialogueNode
    {
        private readonly List<DialoguePiece> dialoguePieces = new();
        private readonly Dictionary<string, DialoguePiece> dialoguePieceMap = new();
        public IReadOnlyList<DialoguePiece> Pieces => dialoguePieces;
        public DialoguePiece this[string pieceName]
        {
            get => dialoguePieceMap[pieceName];
            set
            {
                dialoguePieceMap[pieceName] = value;
            }
        }
        public DialoguePiece GetPiece(string ID)
        {
            if (dialoguePieceMap.ContainsKey(ID))
            {
                return dialoguePieceMap[ID];
            }
            return null;
        }
        private Dialogue Reset()
        {
            dialoguePieces.Clear();
            dialoguePieceMap.Clear();
            ClearModules();
            return this;
        }
        public void AddPiece(DialoguePiece piece)
        {
            dialoguePieces.Add(piece);
            if (dialoguePieceMap.ContainsKey(piece.PieceID))
            {
                Debug.LogWarning($"Dialogue already contain Piece with PieceID {piece.PieceID} !");
            }
            else
            {
                dialoguePieceMap.Add(piece.PieceID, piece);
            }
        }
        public static Dialogue CreateDialogue()
        {
            return NodePoolManager.Instance.GetNode<Dialogue>().Reset();
        }
    }
}
