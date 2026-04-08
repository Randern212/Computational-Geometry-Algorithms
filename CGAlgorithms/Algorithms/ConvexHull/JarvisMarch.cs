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
            Point startingPoint = points[0];
            foreach (Point p in points)
            {
                if (p.X <= startingPoint.X)
                    startingPoint = p;
            }

            List<Point> hull = new List<Point>();
            hull.Add(startingPoint);
            Point currentPoint = startingPoint;
            Point nextPoint = points[0];
            while(true)
            {
                Line cnLine = new Line(currentPoint,  nextPoint);
                foreach(Point p in points)
                {
                    if (p.Equals(nextPoint))
                        continue;

                    Line npLine = new Line(nextPoint, p);

                    Point cnVector = HelperMethods.GetVector(cnLine);
                    Point npVector = HelperMethods.GetVector(npLine);

                    Enums.TurnType cnTOnpTurn = HelperMethods.CheckTurn(cnVector, npVector);

                    if (cnTOnpTurn == Enums.TurnType.Left)
                        nextPoint = p;
                    else if (cnTOnpTurn == Enums.TurnType.Colinear)
                    {
                        double distP = DistanceSquared(currentPoint, p);
                        double distNext = DistanceSquared(currentPoint, nextPoint);
                        if (distP > distNext)
                            nextPoint = p;
                    }
                }

                hull.Add((nextPoint));
                currentPoint = nextPoint;
                if (nextPoint.Equals(startingPoint))
                    break;
            }   
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
