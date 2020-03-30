﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json.Linq;


namespace SAM.Geometry.Planar
{
    public class Polygon2D : SAMGeometry, IClosed2D, ISegmentable2D, IEnumerable<Point2D>
    {
        private List<Point2D> points;

        public Polygon2D(IEnumerable<Point2D> points)
        {
            this.points = Point2D.Clone(points);
        }

        public Polygon2D(Polygon2D polygon2D)
        {
            this.points = polygon2D.GetPoints();
        }

        public Polygon2D(JObject jObject)
            : base(jObject)
        {

        }

        public List<Segment2D> GetSegments()
        {
            return Create.Segment2Ds(points, true);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public Orientation GetOrientation()
        {
            return Query.Orientation(points, true);
        }

        public int Count
        {
            get
            {
                return points.Count;
            }
        }

        public bool SetOrientation(Orientation orientation)
        {
            if (points == null || points.Count < 3 || orientation == Orientation.Undefined || orientation == Orientation.Collinear)
                return false;

            if (GetOrientation() != orientation)
                Modify.Reverse(points, true);

            return true;
        }

        public override ISAMGeometry Clone()
        {
            return new Polygon2D(this);
        }

        public List<Point2D> Points
        {
            get
            {
                return Point2D.Clone(points);
            }
        }

        public List<Point2D> GetPoints()
        {
            return points.ConvertAll(x => new Point2D(x));
        }

        //Inserts new point on one of the edges (closest to point2D)
        public Point2D Insert(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            List<Segment2D> segment2Ds = GetSegments();
            if (segment2Ds == null || segment2Ds.Count == 0)
                return null;

            int index = -1;
            Point2D point2D_Closest = null;
            double distance_Min = double.MaxValue;

            for (int i = 0; i < segment2Ds.Count; i++)
            {
                Segment2D segment2D = segment2Ds[i];

                Point2D point2D_Closest_Temp = segment2D.Closest(point2D);
                if (point2D_Closest_Temp.AlmostEquals(segment2D[0], tolerance) || point2D_Closest_Temp.AlmostEquals(segment2D[1], tolerance))
                    continue;

                double distance = point2D.Distance(point2D_Closest_Temp);
                if (distance < distance_Min)
                {
                    distance_Min = distance;
                    point2D_Closest = point2D_Closest_Temp;
                    index = i;
                }
            }

            if (index == -1)
                return null;

            Segment2D segment2D_Temp = segment2Ds[index];
            segment2Ds[index] = new Segment2D(segment2D_Temp[0], point2D_Closest);
            segment2Ds.Insert(index + 1, new Segment2D(point2D_Closest, segment2D_Temp[1]));

            points = segment2Ds.ConvertAll(x => x.Start);
            return point2D_Closest;
        }

        public Polyline2D GetPolyline(int index_Start, int count)
        {
            return new Polyline2D(points.GetRange(index_Start, count), false);
        }

        public Polyline2D GetPolyline()
        {
            return new Polyline2D(points, true);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public bool Inside(Point2D point2D)
        {
            return Point2D.Inside(points, point2D);
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }

        public Point2D Closest(Point2D point2D, bool includeEdges)
        {
            if (includeEdges)
                return Query.Closest((ISegmentable2D)this, point2D);

            return Query.Closest(points, point2D);
        }

        public void Reverse(bool keepFirstPoint = true)
        {
            if (points == null || points.Count < 2)
                return;

            if(keepFirstPoint)
            {
                Point2D point2D = points[0];
                points.RemoveAt(0);
                points.Add(point2D);
            }

            points.Reverse();
        }

        public double GetArea()
        {
            return Point2D.GetArea(points);
        }

        public override bool FromJObject(JObject jObject)
        {
            if (jObject == null)
                return false;

            points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points"));
            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Core.Create.JArray(points));
            return jObject;
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
           return new BoundingBox2D(points, offset);
        }

        public Point2D GetInternalPoint2D()
        {
            return Point2D.GetInternalPoint2D(points);
        }

        public Point2D GetCentroid()
        {
            return Query.Centroid(points);
        }

        public bool Move(Vector2D vector2D)
        {
            if (points == null || vector2D == null)
                return false;

            for (int i = 0; i < points.Count; i++)
                points[i] = points[i].GetMoved(vector2D);

            return true;
        }

        public List<Point2D> Intersections(ISegmentable2D segmentable2D)
        {
            return Query.Intersections(this, segmentable2D);
        }

        public bool On(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public int IndexOf(Point2D point2D)
        {
            return points.IndexOf(point2D);
        }

        public bool Reorder(int startIndex)
        {
            List<Point2D> points_New = Core.Modify.Reorder(points, startIndex);
            if (points_New == null)
                return false;

            points = points_New;
            return true;
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public double GetParameter(Point2D point2D, bool inverted = false)
        {
            return Query.Parameter(this, point2D, inverted);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            return Query.Point2D(this, parameter, inverted);
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            return Modify.Trim(this, parameter, inverted);
        }

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }

        public bool SimplifyByAngle(double maxAngle = Core.Tolerance.Angle)
        {
            return Modify.SimplifyByAngle(points, true, maxAngle);
        }

        public IEnumerator<Point2D> GetEnumerator()
        {
            return GetPoints().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
