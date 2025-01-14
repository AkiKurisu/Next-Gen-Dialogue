using System;
using Ceres.Annotations;
using UnityEngine;
namespace Kurisu.NGDT
{
    [Serializable]
    [NodeInfo("Piece is the container of single dialogue fragment")]
    public class Piece : Container
    {
        [SerializeField, DisableCopyValue, CeresLabel("Piece ID"), Tooltip("You don't need to fill in this shared variable because its value will be automatically generated at runtime")]
        private PieceID pieceID;
        
        private NGDS.Piece _pieceCache;
        
        protected override Status OnUpdate()
        {
            Builder.StartWriteNode(_pieceCache);
            for (var i = 0; i < Children.Count; i++)
            {
                var target = Children[i];
                var childStatus = target.Update();
                if (childStatus == Status.Success || target is Option)
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
        public NGDS.Piece EmitPiece()
        {
            _pieceCache = NGDS.Piece.GetPooled();
            _pieceCache.ID = pieceID.Value;
            _pieceCache.Name = pieceID.Name;
            return _pieceCache;
        }
        
        /// <summary>
        /// Cast current piece model
        /// </summary>
        /// <returns></returns>
        public NGDS.Piece CastPiece()
        {
            return _pieceCache;
        }
        
        public override void Abort()
        {
            _pieceCache = null;
        }
    }
}

