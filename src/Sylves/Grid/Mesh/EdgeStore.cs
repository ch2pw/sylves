using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY
using UnityEngine;
#endif


namespace Sylves
{
    /// <summary>
    /// Utility class represented a bundle of half edges extracted from a mesh.
    /// Only unpaired half-edges are stored - any paired off edges are immediately recorded in the moves array.
    /// </summary>
    internal class EdgeStore
    {
        private readonly float tolerance;
        private readonly Vector3Int basePoint;

        // the face, submesh and edge ids, stored by start/end snap ids of the edge.
        private Dictionary<(int, int), (Vector3, Vector3, Cell, CellDir)> unmatchedEdges;
        private Dictionary<Vector3Int, int> snaps;
        private int nextSnap;


        public EdgeStore(float tolerance = MeshDataOperations.DefaultTolerance, Vector3Int basePoint = default)
        {
            this.tolerance = tolerance;
            this.basePoint = basePoint;
            unmatchedEdges = new Dictionary<(int, int), (Vector3, Vector3, Cell, CellDir)>();
            snaps = new Dictionary<Vector3Int, int>();
            nextSnap = 0;
        }

        private EdgeStore(
            Dictionary<(int, int), (Vector3, Vector3, Cell, CellDir)> unmatchedEdges,
            Dictionary<Vector3Int, int> snaps,
            int nextSnap,
            float tolerance,
            Vector3Int basePoint)
        {
            this.unmatchedEdges = unmatchedEdges;
            this.snaps = snaps;
            this.nextSnap = nextSnap;
            this.tolerance = tolerance;
            this.basePoint = basePoint;
        }

        public void MapCells(Func<Cell, Cell> f)
        {
            unmatchedEdges = unmatchedEdges.ToDictionary(kv => kv.Key, kv => (kv.Value.Item1, kv.Value.Item2, f(kv.Value.Item3), kv.Value.Item4));
        }

        public IEnumerable<(Vector3 v1, Vector3 v2, Cell cell, CellDir dir)> UnmatchedEdges
        {
            get
            {
                return unmatchedEdges.Values;
            }
        }

        private static readonly Vector3Int[] Offsets = {
            new Vector3Int(0, 0, 0),
            new Vector3Int(0, 0, 1),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, 1, 1),
            new Vector3Int(1, 0, 0),
            new Vector3Int(1, 0, 1),
            new Vector3Int(1, 1, 0),
            new Vector3Int(1, 1, 1),
        };

        public int SnapVertex(Vector3 v1)
        {
            var v1i = Vector3Int.FloorToInt((v1 - basePoint) / tolerance);
            foreach (var o1 in Offsets)
            {
                var w1 = v1i + o1;
                if (snaps.TryGetValue(w1, out var snap))
                {
                    return snap;
                }
            }

            // We use an offset when *storing* the vertex, to avoid boundary issues
            v1i = Vector3Int.FloorToInt((v1 - basePoint) / tolerance + 0.5f * Vector3.one);
            if (!snaps.TryGetValue(v1i, out var newSnap))
            {
                newSnap = nextSnap;
                nextSnap++;
                snaps.Add(v1i, newSnap);
            }
            return newSnap;
        }

        // Attempts to pair the new edge with the unmapped edges.
        // On success, adds it to moves and returns true.
        public bool MatchEdge(int v1snap, int v2snap, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves, bool clearEdge = true)
        {
            if (unmatchedEdges.TryGetValue((v2snap, v1snap), out var match))
            {
                // Edges match, add moves in both directions
                var (_, _, cell2, dir2) = match;
                moves.Add((cell, dir), (cell2, dir2, new Connection()));
                moves.Add((cell2, dir2), (cell, dir, new Connection()));
                if (clearEdge)
                {
                    unmatchedEdges.Remove((v2snap, v1snap));
                }
                return true;
            }

            if (unmatchedEdges.TryGetValue((v1snap, v2snap), out match))
            {
                // Same as above, but with a mirrored connection
                var (_, _, cell2, dir2) = match;
                moves.Add((cell, dir), (cell2, dir2, new Connection { Mirror = true }));
                moves.Add((cell2, dir2), (cell, dir, new Connection { Mirror = true }));
                if (clearEdge)
                {
                    unmatchedEdges.Remove((v1snap, v2snap));
                }
                return true;
            }
            return false;
        }

        // MatchEdge, and if it fails, adds the edge to unmatchedEdges
        public void AddEdge(int v1snap, int v2snap, Vector3 v1, Vector3 v2, Cell cell, CellDir dir, IDictionary<(Cell, CellDir), (Cell, CellDir, Connection)> moves)
        {
            if (!MatchEdge(v1snap, v2snap, cell, dir, moves))
            {
                unmatchedEdges.Add((v1snap, v2snap), (v1, v2, cell, dir));
            }
        }

        public EdgeStore Clone()
        {
            return new EdgeStore(unmatchedEdges.ToDictionary(x => x.Key, x => x.Value),
                snaps.ToDictionary(x => x.Key, x => x.Value),
                nextSnap,
                tolerance,
                basePoint);
        }
    }
}
