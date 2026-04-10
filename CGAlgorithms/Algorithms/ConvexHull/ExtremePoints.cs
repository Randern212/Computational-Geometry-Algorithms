using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class ExtremePoints : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons, ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            points = points.GroupBy(p => new { X = Math.Round(p.X, 6), Y = Math.Round(p.Y, 6) }).Select(g => g.First()).ToList();

            int n = points.Count;

            for (int i = 0; i < n; i++)
            {
                bool isExtreme = true;

                for (int j = 0; j < n && isExtreme; j++)
                {
                    for (int k = 0; k < n && isExtreme; k++)
                    {
                        if (i == j || i == k || j == k)
                            continue;

                        if (points[j].Equals(points[k]))
                            continue;

                        if (HelperMethods.PointOnSegment(points[i], points[j], points[k]))
                        {
                            double minX = Math.Min(points[j].X, points[k].X);
                            double maxX = Math.Max(points[j].X, points[k].X);
                            double minY = Math.Min(points[j].Y, points[k].Y);
                            double maxY = Math.Max(points[j].Y, points[k].Y);

                            if ((points[i].X > minX && points[i].X < maxX) &&
                                (points[i].Y > minY && points[i].Y < maxY))
                            {
                                isExtreme = false;
                            }
                        }
                    }
                }

                for (int j = 0; j < n && isExtreme; j++)
                {
                    for (int k = 0; k < n && isExtreme; k++)
                    {
                        for (int l = 0; l < n && isExtreme; l++)
                        {
                            if (i == j || i == k || i == l ||
                                j == k || j == l || k == l)
                                continue;

                            var turn = HelperMethods.CheckTurn(new Line(points[j], points[k]), points[l]);

                            if (turn == Enums.TurnType.Colinear)
                                continue;

                            var result = HelperMethods.PointInTriangle(points[i], points[j], points[k], points[l]);

                            if (result == Enums.PointInPolygon.Inside ||
                                result == Enums.PointInPolygon.OnEdge)
                            {
                                isExtreme = false;
                            }
                        }
                    }
                }

                if (isExtreme)
                {
                    bool exists = false;
                    foreach (var p in outPoints)
                    {
                        if (p.Equals(points[i]))
                        {
                            exists = true;
                            break;
                        }
                    }

                    if (!exists)
                        outPoints.Add(points[i]);
                }
            }
        }

        public override string ToString()
        {
            return "Convex Hull - Extreme Points";
        }
    }
}

