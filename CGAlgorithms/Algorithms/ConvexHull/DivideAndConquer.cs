using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class DivideAndConquer : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons,
                                         ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            // Remove duplicates
            points = points.GroupBy(p => new { p.X, p.Y }).Select(g => g.First()).ToList();
            if (points.Count <= 1)
            {
                outPoints = points;
                return;
            }

            // Check if all collinear
            bool allCollinear = true;
            for (int i = 2; i < points.Count; i++)
                if (HelperMethods.Turn(points[0], points[1], points[i]) != Enums.TurnType.Colinear)
                { allCollinear = false; break; }

            if (allCollinear)
            {
                var sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
                outPoints = new List<Point> { sorted[0], sorted.Last() };
                if (outPoints[0].Equals(outPoints[1])) outPoints = new List<Point> { outPoints[0] };
                return;
            }

            // Sort by X then Y
            List<Point> sortedPoints = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            outPoints = DivideAndConquerHull(sortedPoints);
        }

        private List<Point> DivideAndConquerHull(List<Point> pts)
        {
            if (pts.Count <= 3)
            {
                // Base case: return convex hull (Monotone Chain on small set)
                return MonotoneChain(pts);
            }

            int mid = pts.Count / 2;
            List<Point> left = pts.Take(mid).ToList();
            List<Point> right = pts.Skip(mid).ToList();

            List<Point> leftHull = DivideAndConquerHull(left);
            List<Point> rightHull = DivideAndConquerHull(right);

            // Merge by computing convex hull of both hulls together
            List<Point> combined = new List<Point>();
            combined.AddRange(leftHull);
            combined.AddRange(rightHull);
            return MonotoneChain(combined);
        }

        // Andrew's Monotone Chain algorithm - O(n log n)
        private List<Point> MonotoneChain(List<Point> pts)
        {
            if (pts.Count <= 1) return pts;
            List<Point> sorted = pts.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            List<Point> lower = new List<Point>();
            foreach (var p in sorted)
            {
                while (lower.Count >= 2 && HelperMethods.Turn(lower[lower.Count - 2], lower[lower.Count - 1], p) != Enums.TurnType.Left)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            List<Point> upper = new List<Point>();
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                var p = sorted[i];
                while (upper.Count >= 2 && HelperMethods.Turn(upper[upper.Count - 2], upper[upper.Count - 1], p) != Enums.TurnType.Left)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            // Remove duplicate endpoints
            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);

            return lower;
        }

        public override string ToString()
        {
            return "Convex Hull - Divide & Conquer";
        }

    }
}
