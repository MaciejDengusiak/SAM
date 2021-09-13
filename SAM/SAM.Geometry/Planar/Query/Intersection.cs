﻿using ClipperLib;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Geometry.Planar
{
    public static partial class Query
    {
        //Intersection of the sets A and B, denoted A ∩ B, is the set of all objects that are members of both A and B. The intersection of {1, 2, 3} and {2, 3, 4} is the set {2, 3}

        public static List<Polygon2D> Intersection(this Polygon2D polygon2D_1, Polygon2D polygon2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            if (polygon2D_1 == null || polygon2D_2 == null)
                return null;

            List<IntPoint> intPoints_1 = Convert.ToClipper((ISegmentable2D)polygon2D_1, tolerance);
            List<IntPoint> intPoints_2 = Convert.ToClipper((ISegmentable2D)polygon2D_2, tolerance);

            Clipper clipper = new Clipper();
            clipper.AddPath(intPoints_1, PolyType.ptSubject, true);
            clipper.AddPath(intPoints_2, PolyType.ptClip, true);

            List<List<IntPoint>> intPointsList = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctIntersection, intPointsList, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            if (intPointsList == null)
                return null;

            List<Polygon2D> result = new List<Polygon2D>();
            if (intPointsList.Count == 0)
                return result;

            foreach (List<IntPoint> intPoints in intPointsList)
                result.Add(new Polygon2D(intPoints.ToSAM(tolerance)));

            return result;
        }

        public static T Intersection<T>(this Segment2D segment2D_1, Segment2D segment2D_2, double tolerance = Core.Tolerance.MicroDistance) where T: ISAMGeometry2D
        {
            if(segment2D_1 == null || segment2D_2 == null)
            {
                return default;
            }

            LineString lineString_1 = segment2D_1.ToNTS(tolerance);
            LineString lineString_2 = segment2D_2.ToNTS(tolerance);
            
            NetTopologySuite.Geometries.Geometry geometry = lineString_1?.Intersection(lineString_2);
            if (geometry == null || geometry.IsEmpty)
            {
                return default;
            }


            if (geometry is LineString)
            {
                Polyline2D polyline2D = ((LineString)geometry).ToSAM(tolerance);
                if (polyline2D.Points.Count == 2 && typeof(T).IsAssignableFrom(typeof(Segment2D)))
                    return (T)(object)(new Segment2D(polyline2D[0], polyline2D[1]));
            }
            else if(geometry is Point)
            {
                Point2D point2D = ((Point)geometry).ToSAM(tolerance);
                if (typeof(T) == typeof(Point2D))
                    return (T)(object)point2D;
            }

            return default;


            //if (segment2D_1 == null || segment2D_2 == null)
            //    return null;

            //if (!Collinear(segment2D_1, segment2D_2))
            //    return null;

            //bool on_1 = segment2D_1.On(segment2D_2[0], tolerance);
            //bool on_2 = segment2D_1.On(segment2D_2[1], tolerance);
            //bool on_3 = segment2D_2.On(segment2D_1[0], tolerance);
            //bool on_4 = segment2D_2.On(segment2D_1[1], tolerance);

            //if (!on_1 && !on_2 && !on_3 && !on_4)
            //    return null;

            //List<Point2D> point2Ds = new List<Point2D>() { segment2D_1[0], segment2D_1[1], segment2D_2[0], segment2D_2[1] };

            //Point2D point2D_1;
            //Point2D point2D_2;
            //ExtremePoints(point2Ds, out point2D_1, out point2D_2);
            //if (point2D_1 == null || point2D_2 == null)
            //    return null;

            //Modify.SortByDistance(point2Ds, point2D_1);

            //for (int i = 0; i < point2Ds.Count - 1; i++)
            //{
            //    Point2D point2D_Mid = point2Ds[i].Mid(point2Ds[i + 1]);
            //    if (segment2D_1.On(point2D_Mid) && segment2D_2.On(point2D_Mid) && point2Ds[i].Distance(point2Ds[i + 1]) > tolerance)
            //        return new Segment2D(point2Ds[i], point2Ds[i + 1]);
            //}

            //return null;
        }

        public static List<Face2D> Intersection(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance)
        {
            return Intersection<Face2D>(face2D_1, face2D_2, tolerance);
        }

        public static List<T> Intersection<T>(this Face2D face2D_1, Face2D face2D_2, double tolerance = Core.Tolerance.MicroDistance) where T: ISAMGeometry2D
        {
            if(face2D_1 == null || face2D_2 == null)
            {
                return null;
            }

            List<Face2D> face2Ds_1 = face2D_1.FixEdges(tolerance);
            if(face2Ds_1 == null || face2Ds_1.Count == 0)
            {
                face2Ds_1 = new List<Face2D>() { face2D_1};
            }

            List<Face2D> face2Ds_2 = face2D_2.FixEdges(tolerance);
            if (face2Ds_2 == null || face2Ds_2.Count == 0)
            {
                face2Ds_2 = new List<Face2D>() { face2D_2 };
            }

            List<NetTopologySuite.Geometries.Geometry> geometries = new List<NetTopologySuite.Geometries.Geometry>();
            foreach (Face2D face2D_1_Temp in face2Ds_1)
            {
                if(face2D_1_Temp == null || face2D_1_Temp.GetArea() < tolerance)
                {
                    continue;
                }

                Polygon polygon_1 = face2D_1_Temp?.ToNTS(tolerance);
                if (polygon_1 == null)
                {
                    continue;
                }

                foreach (Face2D face2D_2_Temp in face2Ds_2)
                {
                    if (face2D_2_Temp == null || face2D_2_Temp.GetArea() < tolerance)
                    {
                        continue;
                    }

                    Polygon polygon_2 = face2D_2_Temp?.ToNTS(tolerance);
                    if (polygon_2 == null)
                    {
                        continue;
                    }

                    List<Polygon> polygons_Snap = Snap(polygon_1, polygon_2, tolerance);
                    if (polygons_Snap != null && polygons_Snap.Count == 2)
                    {
                        polygon_1 = polygons_Snap[0];
                        polygon_2 = polygons_Snap[1];
                    }

                    NetTopologySuite.Geometries.Geometry geometry = polygon_1.Intersection(polygon_2);
                    if (geometry == null || geometry.IsEmpty)
                    {
                        continue;
                    }

                    List<NetTopologySuite.Geometries.Geometry> geometries_Temp = geometry is GeometryCollection ? ((GeometryCollection)geometry).Geometries?.ToList() : new List<NetTopologySuite.Geometries.Geometry>() { geometry };
                    if (geometries_Temp == null || geometries_Temp.Count == 0)
                    {
                        continue;
                    }

                    geometries.AddRange(geometries_Temp);
                }
            }

            List<ISAMGeometry2D> result = new List<ISAMGeometry2D>();
            foreach (NetTopologySuite.Geometries.Geometry geometry_Temp in geometries)
            {
                if (geometry_Temp is MultiPolygon)
                {
                    result.AddRange(((MultiPolygon)geometry_Temp).ToSAM());
                }
                else if (geometry_Temp is Polygon)
                {
                    Face2D face2D = ((Polygon)geometry_Temp).ToSAM();
                    if (face2D != null)
                    {
                        result.Add(face2D);
                    }
                }
                else if(geometry_Temp is LineString)
                {
                    result.Add(((LineString)geometry_Temp).ToSAM(tolerance));
                }
                else if(geometry_Temp is LinearRing)
                {
                    result.Add(((LinearRing)geometry_Temp).ToSAM(tolerance));
                }

            }

            return result.FindAll(x => x is T).ConvertAll(x => (T)x);
        }

        public static List<Segment2D> Intersection(this Face2D face2D, Segment2D segment2D, double tolerance = Core.Tolerance.MicroDistance)
        {
            if(face2D == null || segment2D == null)
            {
                return null;
            }

            if(!segment2D.IsValid() || segment2D.GetLength() < tolerance)
            {
                return null;
            }

            LineString lineString = segment2D?.ToNTS(tolerance);
            if (lineString == null)
                return null;

            List<Face2D> face2Ds = face2D.FixEdges(tolerance);
            if (face2Ds == null || face2Ds.Count == 0)
            {
                face2Ds = new List<Face2D>() { face2D };
            }

            List<Segment2D> result = new List<Segment2D>();
            foreach(Face2D face2D_Temp in face2Ds)
            {
                Polygon polygon = face2D_Temp?.ToNTS(tolerance);
                if (polygon == null)
                {
                    continue;
                }

                NetTopologySuite.Geometries.Geometry geometry = polygon.Intersection(lineString);
                if (geometry == null || !geometry.IsValid || geometry.IsEmpty)
                {
                    continue;
                }

                if(geometry is LineString)
                {
                    List<Segment2D> segment2Ds_Temp = ((LineString)geometry).ToSAM()?.GetSegments();
                    if(segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
                    {
                        result.AddRange(segment2Ds_Temp);
                    }
                }
                else if(geometry is MultiLineString)
                {
                    List<Polyline2D> polyline2Ds = ((MultiLineString)geometry).ToSAM();
                    if(polyline2Ds != null && polyline2Ds.Count != 0)
                    {
                        foreach(Polyline2D polyline2D in polyline2Ds)
                        {
                            List<Segment2D> segment2Ds_Temp = polyline2D?.GetSegments();
                            if (segment2Ds_Temp != null && segment2Ds_Temp.Count != 0)
                            {
                                result.AddRange(segment2Ds_Temp);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<T> Intersection<T>(this IClosed2D closed2D_1, IClosed2D closed2D_2, double tolerance = Core.Tolerance.MicroDistance) where T : ISAMGeometry2D
        {
            if(closed2D_1 == null || closed2D_2 == null)
            {
                return null;
            }

            Face2D face2D_1 = null;
            if(closed2D_1 is Face2D)
            {
                face2D_1 = (Face2D)closed2D_1;
            }
            else
            {
                face2D_1 = new Face2D(closed2D_1);
            }

            Face2D face2D_2 = null;
            if (closed2D_2 is Face2D)
            {
                face2D_2 = (Face2D)closed2D_2;
            }
            else
            {
                face2D_2 = new Face2D(closed2D_2);
            }

            return Intersection<T>(face2D_1, face2D_2, tolerance);
        }
    }
}