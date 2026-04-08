using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class JarvisMarch : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons, ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            outPoints.Clear();

            if (points == null || points.Count == 0)
                return;
            if (points.Count <= 3)
            {
                outPoints.AddRange(points.Distinct());
                return;
            }

            // 1. Find the leftmost lowest point (lowest Y, then smallest X)
            Point start = points[0];
            foreach (Point p in points)
            {
                if (p.Y < start.Y || (p.Y == start.Y && p.X < start.X))
                    start = p;
            }

            List<Point> hull = new List<Point>();
            Point current = start;

            do
            {
                hull.Add(current);
                Point next = null;

                // Find the most counter‑clockwise point relative to 'current'
                foreach (Point p in points)
                {
                    if (p.Equals(current))
                        continue;

                    if (next == null)
                    {
                        next = p;
                        continue;
                    }

                    // Vectors from current to p and from current to next
                    Point vecP = HelperMethods.GetVector(new Line(current, p));
                    Point vecNext = HelperMethods.GetVector(new Line(current, next));

                    Enums.TurnType turn = HelperMethods.CheckTurn(vecP, vecNext);

                    if (turn == Enums.TurnType.Left)
                    {
                        // p is more counter‑clockwise than next
                        next = p;
                    }
                    else if (turn == Enums.TurnType.Colinear)
                    {
                        // Collinear – keep the farthest point
                        double distP = DistanceSquared(current, p);
                        double distNext = DistanceSquared(current, next);
                        if (distP > distNext)
                            next = p;
                    }
                }

                current = next;
            } while (!current.Equals(start));

            // Remove duplicate start point (added again at the end)
            if (hull.Count > 1 && hull.Last().Equals(hull.First()))
                hull.RemoveAt(hull.Count - 1);

            outPoints = hull;
        } 
        
        private double DistanceSquared(Point a, Point b)
        {
            double dx = a.X - b.X;
            double dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }
        public override string ToString()
        {
            return "Convex Hull - Jarvis March";
        }
    }
}
