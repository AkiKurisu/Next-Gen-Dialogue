using Kurisu.NGDS;
using UnityEngine;
namespace Kurisu.NGDT
{
    [AkiInfo("Piece is the container of single dialogue fragment")]
    public class Piece : Container
    {
        [SerializeField, CopyDisable, AkiLabel("Piece ID"), Tooltip("You don't need to fill in this shared variable because its value will be automatically generated at runtime")]
        private PieceID pieceID;
        private DialoguePiece pieceCache;
        protected override void OnAwake()
        {
            InitVariable(pieceID);
        }
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
        public DialoguePiece GetPiece()
        {
            pieceCache = DialoguePiece.CreatePiece();
            pieceCache.PieceID = pieceID.Value;
            return pieceCache;
        }
        public override void Abort()
        {
            pieceCache = null;
        }
    }
}

