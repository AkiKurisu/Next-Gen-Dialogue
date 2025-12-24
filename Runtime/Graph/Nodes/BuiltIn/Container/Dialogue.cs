using System;
using System.Collections.Generic;
using Ceres.Annotations;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [NodeInfo("Dialogue is the main container of dialogue pieces")]
    public class Dialogue : ContainerNode, IDialogueContainer
    {
#if UNITY_EDITOR
        // Just to know which is referenced in graph, should have better solution
        [HideInGraphEditor, SerializeField]
        internal List<string> referencePieces;
#endif
        private NextGenDialogue.Dialogue _dialogueCache;
        
        private readonly Dictionary<string, Piece> _pieceMap = new();
        
        private readonly HashSet<string> _visitedPieceID = new();
        
        public Status Update(IEnumerable<Piece> allPieces)
        {
            _dialogueCache = NextGenDialogue.Dialogue.GetPooled();
            foreach (var piece in allPieces)
            {
                var dialoguePiece = piece.EmitPiece();
                _pieceMap[dialoguePiece.ID] = piece;
                // Assert PieceID should be unique
                _dialogueCache.AddModule(dialoguePiece);
            }
            return Update();
        }
        
        protected sealed override Status OnUpdate()
        {
            Builder.StartWriteNode(_dialogueCache);
            foreach (var child in Children)
            {
                if (child is not Piece)
                    child.Update();
            }
            Builder.EndWriteNode();
            DialogueSystem.Get().StartDialogue(this);
            return Status.Success;
        }
        
        NextGenDialogue.Piece IDialogueContainer.GetNext(string id)
        {
#if UNITY_EDITOR
            Graph.Root.UpdateEditor?.Invoke();
#endif
            if (_visitedPieceID.Contains(id))
            {
                _dialogueCache.GetPiece(id).Dispose();
                _dialogueCache[id] = _pieceMap[id].EmitPiece();
            }
            var newPiece = _dialogueCache.GetPiece(id);
            _visitedPieceID.Add(newPiece.ID);
            _pieceMap[id].Update();
            return newPiece;
        }
        
        NextGenDialogue.Piece IDialogueContainer.GetFirst()
        {
#if UNITY_EDITOR
            Graph.Root.UpdateEditor?.Invoke();
#endif
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is not Piece piece) continue;
                var dialoguePiece = piece.EmitPiece();
                _visitedPieceID.Add(dialoguePiece.ID);
                var status = _pieceMap[dialoguePiece.ID].Update();
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
        public NextGenDialogue.Dialogue ToDialogue()
        {
            return _dialogueCache;
        }
        
        /// <summary>
        /// Get runtime piece map for fast lookup
        /// </summary>
        /// <returns></returns>
        public IReadOnlyDictionary<string, Piece> ToReadOnlyPieceMap()
        {
            return _pieceMap;
        }
    }
}

