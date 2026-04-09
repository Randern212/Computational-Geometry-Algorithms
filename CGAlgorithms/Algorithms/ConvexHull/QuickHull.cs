using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class QuickHull : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons, ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            outPoints.Clear();

            if (points == null || points.Count == 0)
                return;

            points = points.Distinct().ToList();

            if (points.Count <= 3)
            {
                outPoints.AddRange(points);
                return;
            }

            Point minX = points[0];
            Point maxX = points[0];
            foreach (var p in points)
            {
                if (p.X < minX.X) minX = p;
                if (p.X > maxX.X) maxX = p;
            }

            List<Point> upperSet = new List<Point>();
            List<Point> lowerSet = new List<Point>();

            foreach (var p in points)
            {
                if (p.Equals(minX) || p.Equals(maxX)) continue;

                double cross = Cross(minX, maxX, p);
                if (cross > 0)
                    upperSet.Add(p);
                else if (cross < 0)
                    lowerSet.Add(p);
            }

            List<Point> hull = new List<Point>();
            hull.Add(minX);
            BuildHull(upperSet, minX, maxX, hull);
            hull.Add(maxX);
            BuildHull(lowerSet, maxX, minX, hull);

            hull = hull.Distinct().ToList();

            if (hull.Count >= 3 && Cross(hull[0], hull[1], hull[2]) < 0)
                hull.Reverse();

            outPoints = hull;
        }

        private void BuildHull(List<Point> points, Point A, Point B, List<Point> hull)
        {
            if (points.Count == 0) return;

            double maxDist = 0;
            Point farthest = points[0];
            foreach (var p in points)
            {
                double dist = DistanceFromLine(A, B, p);
                if (dist > maxDist)
                {
                    maxDist = dist;
                    farthest = p;
                }
            }

            hull.Add(farthest);

            List<Point> leftOfAF = new List<Point>();
            List<Point> rightOfFB = new List<Point>();

            foreach (var p in points)
            {
                if (p.Equals(farthest)) continue;

                if (Cross(A, farthest, p) > 0)
                    leftOfAF.Add(p);
                if (Cross(farthest, B, p) > 0)
                    rightOfFB.Add(p);
            }

            BuildHull(leftOfAF, A, farthest, hull);
            BuildHull(rightOfFB, farthest, B, hull);
        }

        private double Cross(Point a, Point b, Point c)
        {
            return (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);
        }

        private double DistanceFromLine(Point A, Point B, Point p)
        {
            return Math.Abs(Cross(A, B, p));
        }

        public override string ToString()
        {
            return "Convex Hull - Quick Hull";
        }
    }
}
