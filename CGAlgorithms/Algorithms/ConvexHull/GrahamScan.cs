using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class GrahamScan : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons, ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            List<Point> working_Points = new List<Point>();
            foreach (Point point in points)
            {
                if (!working_Points.Contains(point)) working_Points.Add(point);
            }
            Stack<Point> gstack = new Stack<Point>();

            Point first_point = working_Points[0];
            for (int i = 1; i < working_Points.Count; i++)
            {
                if (working_Points[i].Y < first_point.Y || (working_Points[i].Y == first_point.Y && working_Points[i].X < first_point.X))
                {
                    first_point = working_Points[i];
                }
            }
            // remov
            gstack.Push(first_point);
            working_Points.Remove(first_point);
            
            int orderedworkingPoints_counter = 0;
            var orderedworkingPoints = working_Points.OrderBy(p => Calculating_Angles(first_point, p)).ToList();
            
            if (orderedworkingPoints.Count >= 1)
            {
                foreach (Point point in orderedworkingPoints) { System.Console.WriteLine("(" + point.X + "," + point.Y + ")"); }
                gstack.Push(orderedworkingPoints[orderedworkingPoints_counter]);
                orderedworkingPoints_counter++;
            }

            while (orderedworkingPoints.Count > 1 && orderedworkingPoints_counter < orderedworkingPoints.Count)
            {
                Point top = gstack.Pop();
                Line segment = new Line(gstack.First(), top);

                while (gstack.Count > 1 && HelperMethods.CheckTurn(segment, orderedworkingPoints[orderedworkingPoints_counter]) != Enums.TurnType.Left)
                {
                    top = gstack.Pop();
                    segment = new Line(gstack.First(), top);
                }
                gstack.Push(top);
                gstack.Push(orderedworkingPoints[orderedworkingPoints_counter]);
                orderedworkingPoints_counter++;
            }
            if (orderedworkingPoints.Count > 1)
            {
                Point top_special = gstack.Pop();
                Line segment_special = new Line(gstack.First(), top_special);
                if (HelperMethods.CheckTurn(segment_special, first_point) != Enums.TurnType.Colinear)
                {
                    gstack.Push(top_special);
                }
            }
            outPoints = gstack.ToList();
        }

        static double Calculating_Angles(Point reference, Point point)
        {
            return Math.Atan2(point.Y - reference.Y, point.X - reference.X);
        }
        public override string ToString()
        {
            return "Convex Hull - Graham Scan";
        }
    }
}