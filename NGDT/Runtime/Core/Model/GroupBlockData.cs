using System.Collections.Generic;
using UnityEngine;
namespace Kurisu.NGDT
{
    [System.Serializable]
    public class GroupBlockData
    {
        public List<string> ChildNodes = new();
        public Vector2 Position;
        public string Title = "Node Block";
    }
}
