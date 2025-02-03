using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
namespace NextGenDialogue
{
    public class Dialogue : Node
    {
        private static readonly ObjectPool<Dialogue> Pool = new(() => new Dialogue(), (d) => d.IsPooled = true, (d) => d.Reset());
        
        private readonly List<Piece> _dialoguePieces = new();
        
        private readonly Dictionary<string, Piece> _dialoguePieceMap = new();
        
        public IReadOnlyList<Piece> Pieces => _dialoguePieces;
        
        public Piece this[string pieceName]
        {
            get => _dialoguePieceMap[pieceName];
            set => _dialoguePieceMap[pieceName] = value;
        }
        
        public Piece GetPiece(string id)
        {
            return _dialoguePieceMap.GetValueOrDefault(id);
        }
        
        private Dialogue Reset()
        {
            IsPooled = false;
            _dialoguePieces.Clear();
            _dialoguePieceMap.Clear();
            ClearModules();
            return this;
        }
        
        public void AddPiece(Piece piece)
        {
            _dialoguePieces.Add(piece);
            if (!_dialoguePieceMap.TryAdd(piece.ID, piece))
            {
                Debug.LogWarning($"Dialogue already contain Piece with PieceID {piece.ID} !");
            }
        }
        
        protected override void OnModuleAdd(IDialogueModule module)
        {
            if (module is Piece piece)
            {
                AddPiece(piece);
            }
        }
        
        public static Dialogue GetPooled()
        {
            return Pool.Get();
        }
        
        protected override void OnDispose()
        {
            if (IsPooled)
            {
                Pool.Release(this);
            }
        }
    }
}
