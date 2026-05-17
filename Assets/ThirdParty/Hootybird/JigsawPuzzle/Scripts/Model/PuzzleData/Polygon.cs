using System;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    [Serializable]
    public struct Polygon
    {
        public Vector2Int location;
        public Edge[] edges;

        public Edge this[int index]
        {
            get => edges[index];
            set => edges[index] = value;
        }

        public int EdgeCount => edges.Length;
    }
}
