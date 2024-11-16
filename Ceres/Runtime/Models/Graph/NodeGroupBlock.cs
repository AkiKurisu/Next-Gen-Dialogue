using System;
using System.Collections.Generic;
using UnityEngine;
namespace Ceres.Graph
{
    [Serializable]
    public class NodeGroupBlock
    {
        public List<string> ChildNodes = new();
        public Vector2 Position;
        public string Title = "Node Block";
    }
}
