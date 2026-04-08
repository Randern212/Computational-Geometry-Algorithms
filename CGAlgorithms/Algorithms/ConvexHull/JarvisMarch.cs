using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class JarvisMarch : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons, ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            //search for the point with the least x-axis value
            points.OrderBy(p => p.X);
            Point startingPoint = points.First();

            List<Point> hull = new List<Point>();
            hull.Add(startingPoint);
            Point currentPoint = startingPoint;

            while (true)
            {
                Point nextPoint = null;
                foreach (Point p in points)
                {

                    if (nextPoint == null)
                        nextPoint = p;

                    Line currentToNextLine = new Line(currentPoint, nextPoint);
                    Line currentToPLine = new Line(currentPoint, p);
                    Point cnVector = HelperMethods.GetVector(currentToNextLine);
                    Point cpVector = HelperMethods.GetVector(currentToPLine);

                    Enums.TurnType currentToNextTurn = HelperMethods.CheckTurn(cnVector, cpVector);

                    if (currentToNextTurn == Enums.TurnType.Left)
                    {
                        currentPoint = nextPoint;
                        hull.Add(currentPoint);
                    }
                }
    
                //if current p == p then terminate loop
                if (nextPoint == startingPoint);
                    break;
            }
        }

        public override string ToString()
        {
            return "Convex Hull - Jarvis March";
        }
    }
}
