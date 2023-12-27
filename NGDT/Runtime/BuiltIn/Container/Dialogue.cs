using System.Collections.Generic;
using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Dialogue is the main container of dialogue pieces")]
    public class Dialogue : Container, IDialogueProxy
    {
#if UNITY_EDITOR
        //Just to know which is referenced in graph, should have better solution
        [HideInEditorWindow, SerializeField]
        internal List<string> referencePieces;
#endif
        private NGDS.Dialogue dialogueCache;
        private readonly Dictionary<string, Piece> pieceMap = new();
        private readonly HashSet<string> visitedPieceID = new();
        public Status Update(IEnumerable<Piece> allPieces)
        {
            dialogueCache = NGDS.Dialogue.CreateDialogue();
            foreach (var piece in allPieces)
            {
                var dialoguePiece = piece.EmitPiece();
                pieceMap[dialoguePiece.PieceID] = piece;
                //Assert PieceID should be unique
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
            Builder.EndBuildDialogue(this);
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
        NGDS.Piece IDialogueProxy.GetNext(string ID)
        {
#if UNITY_EDITOR
            Tree.Root.UpdateEditor?.Invoke();
#endif
            if (visitedPieceID.Contains(ID))
            {
                dialogueCache.GetPiece(ID).NodePushPool();
                dialogueCache[ID] = pieceMap[ID].EmitPiece();
            }
            var newPiece = dialogueCache.GetPiece(ID);
            visitedPieceID.Add(newPiece.PieceID);
            pieceMap[ID].Update();
            return newPiece;
        }
        NGDS.Piece IDialogueProxy.GetFirst()
        {
#if UNITY_EDITOR
            Tree.Root.UpdateEditor?.Invoke();
#endif
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is not Piece piece) continue;
                var dialoguePiece = piece.EmitPiece();
                visitedPieceID.Add(dialoguePiece.PieceID);
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
        /// <summary>
        /// Get current dialogue model
        /// </summary>
        /// <returns></returns>
        public NGDS.Dialogue CastDialogue()
        {
            return dialogueCache;
        }
        /// <summary>
        /// Get runtime piece map for fast lookup
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Piece> ToReadOnlyPieceMap()
        {
            return pieceMap;
        }
    }
}

