using System;
using Ceres.Annotations;
using UnityEngine;

namespace NextGenDialogue.Graph
{
    [Serializable]
    [NodeInfo("Define a single dialogue fragment.")]
    public class Piece : ContainerNode
    {
        [SerializeField, DisableCopyValue]
        [CeresLabel("Piece ID")]
        [Tooltip("You don't need to fill in this shared variable because its value will be automatically generated at runtime")]
        private PieceID pieceID;
        
        private NextGenDialogue.Piece _pieceCache;
        
        protected override Status OnUpdate()
        {
            int optionIndex = 0;
            Builder.StartWriteNode(_pieceCache);
            for (var i = 0; i < Children.Count; i++)
            {
                var target = Children[i];
                if (target is Option option)
                {
                    option.OptionIndex = optionIndex++;
                    option.Update();
                    continue;
                }
                var childStatus = target.Update();
                if (childStatus == Status.Success)
                {
                    continue;
                }
                Builder.DisposeWriteNode();
                return Status.Failure;
            }
            Builder.EndWriteNode();
            return Status.Success;
        }
        
        /// <summary>
        /// Emit a new dialogue piece
        /// </summary>
        /// <returns></returns>
        public NextGenDialogue.Piece EmitPiece()
        {
            _pieceCache = NextGenDialogue.Piece.GetPooled();
            _pieceCache.ID = pieceID.Name;
            return _pieceCache;
        }
        
        /// <summary>
        /// Cast current piece model
        /// </summary>
        /// <returns></returns>
        public NextGenDialogue.Piece CastPiece()
        {
            return _pieceCache;
        }
    }
}

