using System.Collections.Generic;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Dialogue is the main container of dialogue pieces")]
    public class Dialogue : Container, IProvideDialogue
    {
#if UNITY_EDITOR
        [HideInEditorWindow, SerializeField]
        internal List<string> referencePieces;
#endif
        private NGDS.Dialogue dialogueCache;
        private readonly Dictionary<string, Piece> pieceMap = new();
        public Status Update(IEnumerable<Piece> allPieces)
        {
            dialogueCache = NGDS.Dialogue.CreateDialogue();
            foreach (var piece in allPieces)
            {
                var dialoguePiece = piece.GetPiece();
                pieceMap[dialoguePiece.PieceID] = piece;
                dialogueCache.AddPiece(dialoguePiece);
            }
            return Update();
        }
        protected sealed override Status OnUpdate()
        {
            Builder.StartWriteNode(dialogueCache);
            foreach (var child in Children)
            {
                if (child is not Piece)
                    child.Update();
            }
            Builder.EndWriteNode();
            Builder.ProvideDialogue(this);
            return Status.Success;
        }
        public override void Abort()
        {
            dialogueCache = null;
            foreach (var piece in pieceMap.Values)
            {
                piece.Abort();
            }
            pieceMap.Clear();
        }
        DialoguePiece IProvideDialogue.GetNext(string ID)
        {
#if UNITY_EDITOR
            Tree.Root.UpdateEditor?.Invoke();
#endif
            var newPiece = dialogueCache.GetPiece(ID);
            pieceMap[ID].Update();
            return newPiece;
        }
        DialoguePiece IProvideDialogue.GetFirst()
        {
#if UNITY_EDITOR
            Tree.Root.UpdateEditor?.Invoke();
#endif
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is not Piece piece) continue;
                var dialoguePiece = piece.GetPiece();
                var status = pieceMap[dialoguePiece.PieceID].Update();
                if (status == Status.Success) return dialoguePiece;
            }
            return null;
        }
#if UNITY_EDITOR
        internal void AddPiece(Piece child, string reference)
        {
            AddChild(child);
            referencePieces.Add(reference);
        }
        internal string ResolvePieceID(int index)
        {
            if (referencePieces == null || index >= referencePieces.Count) return null;
            return referencePieces[index];
        }
#endif
        public NGDS.Dialogue GetDialogue()
        {
            return dialogueCache;
        }

    }
}

