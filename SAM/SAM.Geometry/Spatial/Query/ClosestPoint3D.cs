﻿using System.Collections.Generic;

namespace SAM.Geometry.Spatial
{
    public static partial class Query
    {
        public static Point3D ClosestPoint3D(this IClosedPlanar3D closedPlanar3D, Point3D point3D)
        {
            if (closedPlanar3D == null || point3D == null)
                return null;

            Plane plane = closedPlanar3D.GetPlane();

            Planar.Point2D point2D_Converted = plane.Convert(plane.Project(point3D));

            Planar.IClosed2D externalEdge = null;
            if (closedPlanar3D is Face3D)
                externalEdge = plane.Convert(((Face3D)closedPlanar3D).GetExternalEdge3D());
            else
                externalEdge = plane.Convert(closedPlanar3D);

            if (externalEdge.Inside(point2D_Converted))
                return plane.Convert(point2D_Converted);

            if (externalEdge is Planar.ISegmentable2D)
                return plane.Convert(Planar.Query.Closest((Planar.ISegmentable2D)externalEdge, point2D_Converted));

            return null;
        }

        public static Point3D ClosestPoint3D(this ISegmentable3D segmentable3D, Point3D point3D, out double distance)
        {
            distance = double.NaN;

            if(segmentable3D == null || point3D == null)
            {
                return null;
            }

            List<Segment3D> segment3Ds = segmentable3D.GetSegments();
            if(segment3Ds == null || segment3Ds.Count == 0)
            {
                return null;
            }

            double distance_Min = double.MaxValue;
            Point3D result = null;
            foreach(Segment3D segment3D in segment3Ds)
            {
                if(segment3D == null)
                {
                    continue;
                }

                Point3D point3D_Temp = segment3D.Closest(point3D, true);
                if(point3D_Temp == null)
                {
                    continue;
                }

                double distance_Temp = point3D_Temp.Distance(point3D);
                if (distance_Temp < distance_Min)
                {
                    distance_Min = distance_Temp;
                    distance = distance_Min;
                    result = point3D_Temp;
                }
            }

            return result;
        }

        public static Point3D ClosestPoint3D(this ISegmentable3D segmentable3D, Point3D point3D)
        {
            return ClosestPoint3D(segmentable3D, point3D, out double distance);
        }
    }
}