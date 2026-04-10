using CGUtilities;
using System.Collections.Generic;

namespace CGAlgorithms.Algorithms.ConvexHull
{
    public class ExtremeSegments : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons, ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            if (points.Count <= 2)
            {
                outPoints = points;
                return;
            }

            createConvexHull(points, ref outPoints);

            outPoints = removePointsOnSeg(outPoints);
        }

        private List<Point> removePointsOnSeg(List<Point> inputPoints)
        {
            for (int i = 0; i < inputPoints.Count; i++)
            {
                bool isExtreme = true;
                for (int j = 0; j < inputPoints.Count; j++)
                {
                    for (int k = 0; k < inputPoints.Count; k++)
                    {
                        if (inputPoints[i] != inputPoints[j] && inputPoints[i] != inputPoints[k])
                        {
                            bool onSeg = HelperMethods.PointOnSegment(inputPoints[i], inputPoints[j], inputPoints[k]);
                            if (onSeg)
                            {
                                inputPoints.Remove(inputPoints[i]);
                                isExtreme = false;
                                break;
                            }
                        }
                    }
                    if (!isExtreme)
                        break;
                }
                if (!isExtreme)
                    i--;
            }
            return inputPoints;
        }

        private void createConvexHull(List<Point> inputPoints, ref List<Point> hull)
        {
            for (int i = 0; i < inputPoints.Count; i++)
            {
                for (int j = 0; j < inputPoints.Count; j++)
                {
                    int leftCount = 0, rightCount = 0;
                    for (int k = 0; k < inputPoints.Count; k++)
                    {
                        Line line = new Line(inputPoints[i], inputPoints[j]);
                        if (k != i && k != j)
                        {
                            Enums.TurnType turnTest = HelperMethods.CheckTurn(line, inputPoints[k]);
                            if (turnTest == Enums.TurnType.Left)
                                leftCount++;
                            else if (turnTest == Enums.TurnType.Right)
                                rightCount++;
                        }
                    }
                    if ((leftCount == 0 && rightCount > 0) || (rightCount == 0 && leftCount > 0) && i != j)
                    {
                        if (!hull.Contains(inputPoints[i]))
                            hull.Add(inputPoints[i]);
                        if (!hull.Contains(inputPoints[j]))
                            hull.Add(inputPoints[j]);
                    }
                }
            }

        }
        
        public override string ToString()
        {
            return "Convex Hull - Extreme Segments";
        }
    }
}
