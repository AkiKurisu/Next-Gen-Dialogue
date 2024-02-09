using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Piece is the container of single dialogue fragment")]
    public class Piece : Container
    {
        [SerializeField, CopyDisable, AkiLabel("Piece ID"), Tooltip("You don't need to fill in this shared variable because its value will be automatically generated at runtime")]
        private PieceID pieceID;
        private NGDS.Piece pieceCache;
        protected override Status OnUpdate()
        {
            Builder.StartWriteNode(pieceCache);
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
            pieceCache = NGDS.Piece.CreatePiece();
            pieceCache.PieceID = pieceID.Value;
            return pieceCache;
        }
        /// <summary>
        /// Cast current piece model
        /// </summary>
        /// <returns></returns>
        public NGDS.Piece CastPiece()
        {
            return pieceCache;
        }
        public override void Abort()
        {
            pieceCache = null;
        }
    }
}

