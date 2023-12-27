using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDS
{
    public class Dialogue : Node
    {
        private readonly List<Piece> dialoguePieces = new();
        private readonly Dictionary<string, Piece> dialoguePieceMap = new();
        public IReadOnlyList<Piece> Pieces => dialoguePieces;
        public Piece this[string pieceName]
        {
            get => dialoguePieceMap[pieceName];
            set
            {
                dialoguePieceMap[pieceName] = value;
            }
        }
        public Piece GetPiece(string ID)
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
        public void AddPiece(Piece piece)
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
