using System;
using System.Collections.Generic;

namespace Sylves
{
    /// <summary>
    /// Implementation class for the A* Pathfinding algorithm.
    /// This algorith takes an admissible heuristic, and uses it to find the shortest path
    /// 
    /// </summary>
    public class AStarPathfinding
    {
        // Options
        Cell src;
        IGrid grid;
        Func<Step, float?> stepLengths;
        Func<Cell, float> heuristic;

        // Internal data (retained between Run calls to reduce allocations)
        readonly Heap<Cell, float> heap = new Heap<Cell, float>();
        readonly Dictionary<Cell, float> distances = new Dictionary<Cell, float>();
        readonly Dictionary<Cell, float> fScores = new Dictionary<Cell, float>();
        readonly Dictionary<Cell, Step> steps = new Dictionary<Cell, Step>();

        public AStarPathfinding(IGrid grid, Cell src, Func<Step, float?> stepLengths, Func<Cell, float> heuristic)
        {
            this.src = src;
            this.grid = grid;
            this.stepLengths = stepLengths;
            this.heuristic = heuristic;
        }

        public void Run(Cell target)
        {
            heap.Clear();
            distances.Clear();
            fScores.Clear();
            steps.Clear();

            heap.Insert(src, 0);
            distances[src] = 0;
            fScores[src] = heuristic(src);
            while(heap.Count > 0)
            {
                var lf = heap.PeekKey();
                var cell = heap.Pop();
                var d = distances[cell];
                var f = fScores[cell];

                if (cell == target)
                {
                    // Found the given cell
                    break;
                }

                if (f < lf)
                {
                    // This entry is redundant, we've already visited with a lower priority.
                    continue;
                }

                foreach (var dir in grid.GetCellDirs(cell))
                {
                    if (Step.Create(grid, cell, dir, stepLengths) is Step step)
                    {
                        var d2 = d + step.Length;
                        var dest = step.Dest;
                        if (!distances.TryGetValue(dest, out var d3) || d2 < d3)
                        {
                            distances[dest] = d2;
                            fScores[dest] = d2 + heuristic(dest);
                            steps[dest] = step;
                            heap.Insert(dest, fScores[dest]);
                        }
                    }
                }
            }
        }

        public CellPath ExtractPathTo(Cell target)
        {
            var pathSteps = new List<Step>();
            if (!ExtractPathTo(target, pathSteps))
            {
                return null;
            }
            return new CellPath { Steps = pathSteps };
        }

        /// <summary>
        /// Writes the path to <paramref name="pathSteps"/> (after clearing it).
        /// Returns false if there is no path to the target.
        /// </summary>
        public bool ExtractPathTo(Cell target, List<Step> pathSteps)
        {
            pathSteps.Clear();
            if (target != src && !steps.ContainsKey(target))
            {
                return false;
            }

            var cell = target;
            while (cell != src)
            {
                var step = steps[cell];
                pathSteps.Add(step);
                cell = step.Src;
            }
            pathSteps.Reverse();
            return true;
        }

    }
}
