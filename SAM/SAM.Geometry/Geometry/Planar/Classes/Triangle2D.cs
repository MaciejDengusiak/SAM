﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public class Triangle2D : SAMGeometry, IClosed2D, ISegmentable2D, IReversible
    {
        private Point2D[] points = new Point2D[3];

        public Triangle2D(Point2D point2D_1, Point2D point2D_2, Point2D point2D_3)
        {
            points[0] = point2D_1;
            points[1] = point2D_2;
            points[2] = point2D_3;
        }

        public Triangle2D(Triangle2D triangle2D)
        {
            points = Query.Clone(triangle2D.points).ToArray();
        }

        public Triangle2D(JObject jObject)
            : base(jObject)
        {
        }

        public static implicit operator Triangle2D(Spatial.Triangle3D triangle3D)
        {
            Spatial.Plane plane = triangle3D?.GetPlane();
            if (plane == null)
            {
                return null;
            }

            List<Spatial.Point3D> point3Ds = triangle3D.GetPoints();

            return new Triangle2D(Spatial.Query.Convert(plane, point3Ds[0]), Spatial.Query.Convert(plane, point3Ds[1]), Spatial.Query.Convert(plane, point3Ds[2]));
        }

        public override ISAMGeometry Clone()
        {
            return new Triangle2D(this);
        }

        public bool Contains(Point2D point2D, double offset)
        {
            return Query.Contains(points, point2D, offset);
        }

        public double Distance(ISegmentable2D segmentable2D)
        {
            return Query.Distance(this, segmentable2D);
        }

        public double Distance(Point2D point2D)
        {
            return Query.Distance(this, point2D);
        }

        public override bool FromJObject(JObject jObject)
        {
            points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points")).ToArray();

            return true;
        }

        public double GetArea()
        {
            return Query.Area(points);
            //0.5 * [x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)]
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(points, offset);
        }

        public Point2D GetCentroid()
        {
            return Query.Centroid(points);
        }

        public List<ICurve2D> GetCurves()
        {
            return GetSegments().ConvertAll(x => (ICurve2D)x);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(points[0], points[1], points[2]).GetHashCode();
        }

        public Point2D GetInternalPoint2D(double tolerance = Core.Tolerance.Distance)
        {
            return GetCentroid();
        }

        public double GetLength()
        {
            return GetSegments().ConvertAll(x => x.GetLength()).Sum();
        }

        public Rectangle2D GetMinRectangle()
        {
            return Create.Rectangle2D(points);
        }

        public Orientation GetOrientation()
        {
            List<Point2D> point2Ds = GetPoints();

            return Query.Orientation(point2Ds[0], point2Ds[1], point2Ds[2]);
        }

        public double GetParameter(Point2D point2D, bool inverted = false, double tolerance = Core.Tolerance.Distance)
        {
            return Query.Parameter(this, point2D, inverted, tolerance);
        }

        public double GetPerimeter()
        {
            return points[0].Distance(points[1]) + points[1].Distance(points[2]) + points[2].Distance(points[0]);
        }

        public Point2D GetPoint(double parameter, bool inverted = false)
        {
            return Query.Point2D(this, parameter, inverted);
        }

        public List<Point2D> GetPoints()
        {
            return new List<Point2D>() { new Point2D(points[0]), new Point2D(points[1]), new Point2D(points[2]) };
        }

        public List<Segment2D> GetSegments()
        {
            return new List<Segment2D> { new Segment2D(points[0], points[1]), new Segment2D(points[1], points[2]), new Segment2D(points[2], points[0]) };
        }

        public bool Inside(BoundingBox2D boundingBox2D, double tolerance = Core.Tolerance.Distance)
        {
            return GetBoundingBox().Inside(boundingBox2D, tolerance);
        }

        public bool Inside(Point2D point2D, double tolerance = Core.Tolerance.Distance)
        {
            bool result = Query.Inside(points, point2D);
            if (!result)
                return result;

            return !On(point2D, tolerance);
        }

        public bool Inside(IClosed2D closed2D, double tolerance = Core.Tolerance.Distance)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x, tolerance));

            throw new NotImplementedException();
        }

        public void Mirror(Point2D point2D)
        {
            if (point2D == null)
                return;

            foreach (Point2D point2D_Temp in points)
                point2D_Temp.Mirror(point2D);
        }

        public void Mirror(Segment2D segment2D)
        {
            if (segment2D == null)
                return;

            foreach (Point2D point2D in points)
                point2D.Mirror(segment2D);
        }

        public bool On(Point2D point2D, double tolerance = 1E-09)
        {
            return Query.On(GetSegments(), point2D, tolerance);
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Geometry.Create.JArray(points));

            return jObject;
        }

        public IEnumerable<Triangle2D> Triangulate()
        {
            return new Triangle2D[1] { new Triangle2D(new Point2D(points[0]), new Point2D(points[1]), new Point2D(points[2])) };
        }

        public ISegmentable2D Trim(double parameter, bool inverted = false)
        {
            return Query.Trim(this, parameter, inverted);
        }

        public ISAMGeometry2D GetTransformed(Transform2D transform2D)
        {
            return Query.Transform(this, transform2D);
        }
    }
}