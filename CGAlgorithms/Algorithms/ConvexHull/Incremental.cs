using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class Incremental : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons,
                                 ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            outPoints.Clear();

            // Handle trivial cases
            if (points == null || points.Count == 0)
                return;

            // Remove duplicate points
            points = points.Distinct().ToList();

            if (points.Count <= 3)
            {
                outPoints.AddRange(points);
                return;
            }

            // Sort points lexicographically (by X, then Y)
            List<Point> sorted = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            // Build lower hull
            List<Point> lower = new List<Point>();
            foreach (var p in sorted)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            // Build upper hull
            List<Point> upper = new List<Point>();
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                var p = sorted[i];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            // Combine lower and upper hulls, removing duplicate endpoints
            lower.RemoveAt(lower.Count - 1); // last point of lower is first of upper
            upper.RemoveAt(upper.Count - 1); // last point of upper is first of lower
            List<Point> hull = lower.Concat(upper).ToList();

            // Ensure counter‑clockwise order (needed for some tests)
            if (hull.Count >= 3 && Cross(hull[0], hull[1], hull[2]) < 0)
                hull.Reverse();

            outPoints = hull;
        }

        // Cross product: (b - a) x (c - a)
        private double Cross(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        public override string ToString()
        {
            return "Convex Hull - Incremental";
        }
    }
}
