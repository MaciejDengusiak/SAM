﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public class Triangle2D : SAMGeometry, IClosed2D, ISegmentable2D
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
            points = Point2D.Clone(triangle2D.points).ToArray();
        }

        public Triangle2D(JObject jObject)
            : base(jObject)
        {

        }

        public bool Contains(Point2D point2D, double offset)
        {
            return Point2D.Contains(points, point2D, offset);
        }

        public double GetArea()
        {
            return Point2D.GetArea(points);
        }

        public BoundingBox2D GetBoundingBox(double offset = 0)
        {
            return new BoundingBox2D(points, offset);
        }

        public Point2D GetCentroid()
        {
            return Point2D.GetCentroid(points);
        }

        public Rectangle2D GetMinRectangle()
        {
            return Point2D.GetRectangle2D(points);
        }

        public double GetPerimeter()
        {
            return points[0].Distance(points[1]) + points[1].Distance(points[2]) + points[2].Distance(points[0]);
        }

        public List<Point2D> GetPoints()
        {
            return new List<Point2D>() { new Point2D(points[0]), new Point2D(points[1]), new Point2D(points[2]) };
        }

        public List<Segment2D> GetSegments()
        {
            return new List<Segment2D> { new Segment2D(points[0], points[1]), new Segment2D(points[1], points[2]), new Segment2D(points[2], points[0]) };
        }

        public bool Inside(BoundingBox2D boundingBox2D)
        {
            return this.GetBoundingBox().Inside(boundingBox2D);
        }

        public bool Inside(Point2D point2D)
        {
            return Point2D.Inside(points, point2D);
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

        public IEnumerable<Triangle2D> Triangulate()
        {
            return new Triangle2D[1] { new Triangle2D(new Point2D(points[0]), new Point2D(points[1]), new Point2D(points[2])) };
        }

        public override ISAMGeometry Clone()
        {
            return new Triangle2D(this);
        }

        public bool Inside(IClosed2D closed2D)
        {
            if (closed2D is ISegmentable2D)
                return ((ISegmentable2D)closed2D).GetPoints().TrueForAll(x => Inside(x));

            throw new NotImplementedException();
        }

        public void Reverse()
        {
            points.Reverse();
        }

        public Orientation GetOrientation()
        {
            List<Point2D> point2Ds = GetPoints();

            return Point2D.Orientation(point2Ds[0], point2Ds[1], point2Ds[2]);
        }

        public override bool FromJObject(JObject jObject)
        {
            points = Geometry.Create.ISAMGeometries<Point2D>(jObject.Value<JArray>("Points")).ToArray();

            return true;
        }

        public override JObject ToJObject()
        {
            JObject jObject = base.ToJObject();
            if (jObject == null)
                return null;

            jObject.Add("Points", Geometry.Create.JArray(points));

            return jObject;
        }
    }
}
