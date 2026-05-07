using CGUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CGAlgorithms.Algorithms.SegmentIntersection
{
    class SweepLine : Algorithm
    {
        public override void Run(List<Point> points, List<Line> lines, List<Polygon> polygons,
                                    ref List<Point> outPoints, ref List<Line> outLines, ref List<Polygon> outPolygons)
        {
            outPoints.Clear();
            if (lines == null || lines.Count < 2) return;

            // Convert to internal segment structure
            List<Segment> segments = new List<Segment>();
            foreach (var line in lines)
                segments.Add(new Segment(line.Start, line.End));

            // Event queue (min‑heap)
            MinHeap<Event> eventQueue = new MinHeap<Event>();

            // Add start and end events for each segment
            foreach (var seg in segments)
            {
                eventQueue.Add(new Event(seg.Left.X, EventType.Start, seg));
                eventQueue.Add(new Event(seg.Right.X, EventType.End, seg));
            }

            // Active set – list of segments ordered by their Y at current sweep X
            List<Segment> active = new List<Segment>();
            Dictionary<Segment, int> indexMap = new Dictionary<Segment, int>();

            // Avoid duplicate intersection events
            HashSet<string> processedIntersections = new HashSet<string>();

            // Sweep
            while (eventQueue.Count > 0)
            {
                Event ev = eventQueue.Pop();
                double sweepX = ev.X;

                switch (ev.Type)
                {
                    case EventType.Start:
                        InsertSegment(active, indexMap, ev.Segment, sweepX);
                        CheckNeighbors(active, indexMap, ev.Segment, sweepX, eventQueue, processedIntersections);
                        break;

                    case EventType.End:
                        RemoveSegment(active, indexMap, ev.Segment, sweepX, eventQueue, processedIntersections);
                        break;

                    case EventType.Intersect:
                        SwapSegments(active, indexMap, ev.SegmentA, ev.SegmentB, sweepX);
                        // Check new neighbors for both segments
                        CheckNeighbors(active, indexMap, ev.SegmentA, sweepX, eventQueue, processedIntersections);
                        CheckNeighbors(active, indexMap, ev.SegmentB, sweepX, eventQueue, processedIntersections);
                        outPoints.Add(ev.IntersectionPoint);
                        break;
                }
            }

            // Remove duplicate intersection points (floating point tolerance)
            outPoints = outPoints.Distinct(new PointComparer()).ToList();
        }

        // ------------------------------------------------------------------
        // Segment representation
        // ------------------------------------------------------------------
        private class Segment
        {
            public Point P1, P2;
            public Point Left, Right;  // sorted by X, then Y
            public double MinX, MaxX, MinY, MaxY;

            public Segment(Point a, Point b)
            {
                P1 = a; P2 = b;
                if (a.X < b.X || (Math.Abs(a.X - b.X) < 1e-9 && a.Y < b.Y))
                { Left = a; Right = b; }
                else
                { Left = b; Right = a; }
                MinX = Math.Min(a.X, b.X);
                MaxX = Math.Max(a.X, b.X);
                MinY = Math.Min(a.Y, b.Y);
                MaxY = Math.Max(a.Y, b.Y);
            }

            // Y coordinate on the segment for a given X
            public double GetY(double x)
            {
                if (Math.Abs(P2.X - P1.X) < 1e-9) return P1.Y; // vertical (rare)
                double t = (x - P1.X) / (P2.X - P1.X);
                return P1.Y + t * (P2.Y - P1.Y);
            }
        }

        // ------------------------------------------------------------------
        // Event types and event class
        // ------------------------------------------------------------------
        private enum EventType { Start, End, Intersect }

        private class Event : IComparable<Event>
        {
            public double X;
            public EventType Type;
            public Segment Segment;          // for Start / End
            public Segment SegmentA, SegmentB; // for Intersect
            public Point IntersectionPoint;

            public Event(double x, EventType type, Segment seg)
            {
                X = x; Type = type; Segment = seg;
            }

            public Event(double x, Segment a, Segment b, Point pt)
            {
                X = x; Type = EventType.Intersect;
                SegmentA = a; SegmentB = b; IntersectionPoint = pt;
            }

            public int CompareTo(Event other)
            {
                if (Math.Abs(X - other.X) > 1e-9)
                    return X.CompareTo(other.X);
                // At same X: Start → Intersect → End
                int order = Type.CompareTo(other.Type);
                if (order != 0) return order;
                return 0;
            }
        }

        // ------------------------------------------------------------------
        // Min‑heap (priority queue)
        // ------------------------------------------------------------------
        private class MinHeap<T> where T : IComparable<T>
        {
            private List<T> heap = new List<T>();
            public int Count => heap.Count;
            public void Add(T item)
            {
                heap.Add(item);
                int i = heap.Count - 1;
                while (i > 0)
                {
                    int parent = (i - 1) / 2;
                    if (heap[parent].CompareTo(heap[i]) <= 0) break;
                    Swap(i, parent);
                    i = parent;
                }
            }
            public T Pop()
            {
                T root = heap[0];
                heap[0] = heap[heap.Count - 1];
                heap.RemoveAt(heap.Count - 1);
                Heapify(0);
                return root;
            }
            private void Heapify(int i)
            {
                int left = 2 * i + 1, right = 2 * i + 2, smallest = i;
                if (left < heap.Count && heap[left].CompareTo(heap[smallest]) < 0) smallest = left;
                if (right < heap.Count && heap[right].CompareTo(heap[smallest]) < 0) smallest = right;
                if (smallest != i)
                {
                    Swap(i, smallest);
                    Heapify(smallest);
                }
            }
            private void Swap(int i, int j)
            {
                T tmp = heap[i]; heap[i] = heap[j]; heap[j] = tmp;
            }
        }

        // ------------------------------------------------------------------
        // Active set management
        // ------------------------------------------------------------------
        private int FindInsertIndex(List<Segment> active, Segment seg, double sweepX)
        {
            int lo = 0, hi = active.Count - 1;
            double ySeg = seg.GetY(sweepX);
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                double yMid = active[mid].GetY(sweepX);
                if (ySeg < yMid)
                    hi = mid - 1;
                else
                    lo = mid + 1;
            }
            return lo;
        }

        private void InsertSegment(List<Segment> active, Dictionary<Segment, int> indexMap,
                                   Segment seg, double sweepX)
        {
            int idx = FindInsertIndex(active, seg, sweepX);
            active.Insert(idx, seg);
            UpdateIndices(active, indexMap);
        }

        private void RemoveSegment(List<Segment> active, Dictionary<Segment, int> indexMap,
                                   Segment seg, double sweepX,
                                   MinHeap<Event> eq, HashSet<string> processed)
        {
            int idx = indexMap[seg];
            // Get neighbours before removal
            Segment above = (idx + 1 < active.Count) ? active[idx + 1] : null;
            Segment below = (idx - 1 >= 0) ? active[idx - 1] : null;
            active.RemoveAt(idx);
            UpdateIndices(active, indexMap);
            // When removing, the neighbours become adjacent
            if (above != null && below != null)
                CheckIntersection(above, below, sweepX, eq, processed);
        }

        private void SwapSegments(List<Segment> active, Dictionary<Segment, int> indexMap,
                                  Segment a, Segment b, double sweepX)
        {
            int idxA = indexMap[a];
            int idxB = indexMap[b];
            if (Math.Abs(idxA - idxB) != 1) return;
            active[idxA] = b;
            active[idxB] = a;
            UpdateIndices(active, indexMap);
        }

        private void UpdateIndices(List<Segment> active, Dictionary<Segment, int> indexMap)
        {
            indexMap.Clear();
            for (int i = 0; i < active.Count; i++)
                indexMap[active[i]] = i;
        }

        private void CheckNeighbors(List<Segment> active, Dictionary<Segment, int> indexMap,
                                    Segment seg, double sweepX,
                                    MinHeap<Event> eq, HashSet<string> processed)
        {
            int idx = indexMap[seg];
            if (idx + 1 < active.Count)
                CheckIntersection(seg, active[idx + 1], sweepX, eq, processed);
            if (idx - 1 >= 0)
                CheckIntersection(active[idx - 1], seg, sweepX, eq, processed);
        }

        private void CheckIntersection(Segment a, Segment b, double sweepX,
                                       MinHeap<Event> eq, HashSet<string> processed)
        {
            Point inter;
            if (SegmentIntersection(a, b, out inter))
            {
                double xInter = inter.X;
                // Only add intersection events that occur strictly to the right of the sweep line.
                // (Intersections exactly at the current X are captured as endpoints.)
                if (xInter > sweepX + 1e-9)
                {
                    string key = GetIntersectionKey(a, b);
                    if (!processed.Contains(key))
                    {
                        processed.Add(key);
                        eq.Add(new Event(xInter, a, b, inter));
                    }
                }
            }
        }

        private string GetIntersectionKey(Segment a, Segment b)
        {
            // Use the four endpoints to create a unique key.
            var pts = new[] { a.P1, a.P2, b.P1, b.P2 }
                .OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            return $"{pts[0].X},{pts[0].Y}|{pts[1].X},{pts[1].Y}|{pts[2].X},{pts[2].Y}|{pts[3].X},{pts[3].Y}";
        }

        // ------------------------------------------------------------------
        // Geometry helpers (segment‑segment intersection)
        // ------------------------------------------------------------------
        private bool SegmentIntersection(Segment s1, Segment s2, out Point inter)
        {
            inter = new Point(0, 0);
            Point p1 = s1.P1, p2 = s1.P2, p3 = s2.P1, p4 = s2.P2;

            double o1 = Orient(p1, p2, p3);
            double o2 = Orient(p1, p2, p4);
            double o3 = Orient(p3, p4, p1);
            double o4 = Orient(p3, p4, p2);

            // Proper intersection
            if (o1 * o2 < 0 && o3 * o4 < 0)
            {
                double x1 = p1.X, y1 = p1.Y, x2 = p2.X, y2 = p2.Y;
                double x3 = p3.X, y3 = p3.Y, x4 = p4.X, y4 = p4.Y;
                double denom = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
                if (Math.Abs(denom) < 1e-9) return false;
                double t = ((x1 - x3) * (y3 - y4) - (y1 - y3) * (x3 - x4)) / denom;
                double u = -((x1 - x2) * (y1 - y3) - (y1 - y2) * (x1 - x3)) / denom;
                double ix = x1 + t * (x2 - x1);
                double iy = y1 + t * (y2 - y1);
                inter = new Point(ix, iy);
                return true;
            }

            // Endpoint collinear cases
            if (Math.Abs(o1) < 1e-9 && OnSegment(p1, p2, p3)) { inter = p3; return true; }
            if (Math.Abs(o2) < 1e-9 && OnSegment(p1, p2, p4)) { inter = p4; return true; }
            if (Math.Abs(o3) < 1e-9 && OnSegment(p3, p4, p1)) { inter = p1; return true; }
            if (Math.Abs(o4) < 1e-9 && OnSegment(p3, p4, p2)) { inter = p2; return true; }

            return false;
        }

        private double Orient(Point p, Point q, Point r)
        {
            double val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (Math.Abs(val) < 1e-9) return 0;
            return val > 0 ? 1 : -1;
        }

        private bool OnSegment(Point p, Point q, Point r)
        {
            return r.X >= Math.Min(p.X, q.X) - 1e-9 && r.X <= Math.Max(p.X, q.X) + 1e-9 &&
                   r.Y >= Math.Min(p.Y, q.Y) - 1e-9 && r.Y <= Math.Max(p.Y, q.Y) + 1e-9;
        }

        private class PointComparer : IEqualityComparer<Point>
        {
            public bool Equals(Point a, Point b)
            {
                return Math.Abs(a.X - b.X) < 1e-9 && Math.Abs(a.Y - b.Y) < 1e-9;
            }
            public int GetHashCode(Point p)
            {
                return (p.X * 1000000 + p.Y).GetHashCode();
            }
        }
        public override string ToString()
        {
            return "Sweep Line";
        }
    }
}