﻿using System;
using SAM.Geometry.Spatial;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Guid> JoinExternal(this List<Panel> panels, double elevation, double maxDistance, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            return JoinExternal(panels, elevation, maxDistance, out List<Panel> externalPanels_Old, out List<Panel> externalPanels_New, out List<Geometry.Planar.Polygon2D> externalPolygon2Ds, snapTolerance, tolerance_Angle, tolerance_Distance);
        }

        public static List<Guid> JoinExternal(this List<Panel> panels, double elevation, double maxDistance, out List<Panel> externalPanels_Old, out List<Panel> externalPanels_New, out List<Geometry.Planar.Polygon2D> externalPolygon2Ds, double snapTolerance = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance)
        {
            externalPanels_Old = null;
            externalPanels_New = null;
            externalPolygon2Ds = null;

            if (panels == null)
                return null;

            Plane plane = Plane.WorldXY.GetMoved(new Vector3D(0, 0, elevation)) as Plane;

            Dictionary<Panel, List<Geometry.Planar.ISegmentable2D>> dictionary = panels.SectionDictionary<Geometry.Planar.ISegmentable2D>(plane, tolerance_Distance);

            List<Geometry.Planar.ISegmentable2D> segmentable2Ds = new List<Geometry.Planar.ISegmentable2D>();
            foreach(KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                if(keyValuePair.Value != null)
                {
                    segmentable2Ds.AddRange(keyValuePair.Value);
                }
            }
            
            externalPolygon2Ds = Geometry.Planar.Query.ExternalPolygon2Ds(segmentable2Ds, maxDistance, snapTolerance, tolerance_Distance);
            if (externalPolygon2Ds == null || externalPolygon2Ds.Count == 0)
                return new List<Guid>();

            List<Geometry.Planar.Segment2D> segment2Ds_Polygon3Ds = new List<Geometry.Planar.Segment2D>();
            foreach (Geometry.Planar.Polygon2D polygon2D in externalPolygon2Ds)
            {
                List<Geometry.Planar.Segment2D> segment2Ds_Temp = polygon2D?.GetSegments();
                if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                {
                    continue;
                }

                segment2Ds_Polygon3Ds.AddRange(segment2Ds_Temp);
            }

            List<Geometry.Planar.Segment2D> segment2Ds = new List<Geometry.Planar.Segment2D>(segment2Ds_Polygon3Ds);

            externalPanels_Old = new List<Panel>();
            foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                List<Geometry.Planar.ISegmentable2D> segmentable2Ds_Temp = keyValuePair.Value;
                if (segmentable2Ds_Temp == null || segmentable2Ds_Temp.Count == 0)
                {
                    continue;
                }

                bool external = false;
                foreach (Geometry.Planar.ISegmentable2D segmentable2D in segmentable2Ds_Temp)
                {
                    List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D?.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Geometry.Planar.Segment2D segment2D in segment2Ds_Temp)
                    {
                        Geometry.Planar.Polygon2D polygon2D = externalPolygon2Ds.Find(x => x.On(segment2D.Mid(), tolerance_Distance));
                        if (polygon2D != null)
                        {
                            segment2Ds.Add(segment2D);
                        }

                        if(!external)
                        {
                            Geometry.Planar.BoundingBox2D boundingBox2D = segment2D.GetBoundingBox();
                            foreach(Geometry.Planar.Segment2D segment2D_Polygon2D in segment2Ds_Polygon3Ds)
                            {
                                if(boundingBox2D.InRange(segment2D_Polygon2D.GetBoundingBox(), snapTolerance) && segment2D.Collinear(segment2D_Polygon2D, tolerance_Angle))
                                {
                                    if(segment2D.On(segment2D_Polygon2D[0], snapTolerance) || segment2D.On(segment2D_Polygon2D[1], snapTolerance) || segment2D_Polygon2D.On(segment2D[0], snapTolerance) || segment2D_Polygon2D.On(segment2D[1], snapTolerance))
                                    {
                                        external = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if(external)
                {
                    externalPanels_Old.Add(keyValuePair.Key);
                }
            }

            externalPanels_New = new List<Panel>(externalPanels_Old);

            segment2Ds = Geometry.Planar.Query.Split(segment2Ds, tolerance_Distance);

            foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
            {
                if (keyValuePair.Value == null)
                {
                    continue;
                }

                foreach (Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                {
                    if (segment2Ds == null || segment2Ds.Count == 0)
                    {
                        break;
                    }

                    List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                    if (segment2Ds_Temp == null || segment2Ds_Temp.Count == 0)
                    {
                        continue;
                    }

                    foreach (Geometry.Planar.Segment2D segment2D in segment2Ds_Temp)
                    {
                        if (segment2D == null || segment2Ds == null || segment2Ds.Count == 0)
                        {
                            break;
                        }

                        Geometry.Planar.Point2D point2D_Mid = segment2D.Mid();

                        segment2Ds.RemoveAll(x => x.On(point2D_Mid, snapTolerance));
                    }
                }
            }

            HashSet<Guid> result = new HashSet<Guid>();
            foreach (Geometry.Planar.Segment2D segment2D in segment2Ds)
            {
                Geometry.Planar.Point2D point2D_1 = segment2D[0];
                Geometry.Planar.Point2D point2D_2 = segment2D[1];

                Tuple<Panel, Geometry.Planar.Segment2D> tuple_Temp = null;
                foreach (KeyValuePair<Panel, List<Geometry.Planar.ISegmentable2D>> keyValuePair in dictionary)
                {
                    foreach (Geometry.Planar.ISegmentable2D segmentable2D in keyValuePair.Value)
                    {
                        List<Geometry.Planar.Segment2D> segment2Ds_Temp = segmentable2D.GetSegments();
                        Geometry.Planar.Point2D point2D_Temp = null;
                        Geometry.Planar.Segment2D segment2D_Temp = null;

                        segment2D_Temp = segment2Ds_Temp[0];
                        point2D_Temp = segment2D_Temp[0];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, Geometry.Planar.Segment2D>(keyValuePair.Key, segment2D_Temp);
                                break;
                            }
                        }

                        segment2D_Temp = segment2Ds_Temp[segment2Ds_Temp.Count - 1];
                        point2D_Temp = segment2D_Temp[1];

                        if (point2D_1.AlmostEquals(point2D_Temp, snapTolerance) || point2D_2.AlmostEquals(point2D_Temp, snapTolerance))
                        {
                            if (segment2D.Direction.SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle || segment2D.Direction.GetNegated().SmallestAngle(segment2D_Temp.Direction) < tolerance_Angle)
                            {
                                tuple_Temp = new Tuple<Panel, Geometry.Planar.Segment2D>(keyValuePair.Key, segment2D_Temp);
                                break;
                            }
                        }
                    }

                    if (tuple_Temp != null)
                    {
                        break;
                    }
                }

                if (tuple_Temp == null)
                {
                    continue;
                }

                Panel panel_Old = tuple_Temp.Item1;

                BoundingBox3D boundingBox3D = panel_Old.GetBoundingBox();
                
                Geometry.Planar.Query.ExtremePoints(new Geometry.Planar.Point2D[] { point2D_1, point2D_2, tuple_Temp.Item2[0], tuple_Temp.Item2[1] }, out point2D_1, out point2D_2);

                Plane plane_Bottom = Plane.WorldXY.GetMoved(new Vector3D(0, 0, boundingBox3D.Min.Z)) as Plane;

                Geometry.Planar.Segment2D segment2D_New = new Geometry.Planar.Segment2D(point2D_1, point2D_2);

                Face3D face3D = Geometry.Spatial.Create.Face3D(plane_Bottom.Convert(segment2D_New), boundingBox3D.Max.Z - boundingBox3D.Min.Z);

                Panel panel_New = new Panel(panel_Old.Guid, panel_Old, face3D);

                int index = panels.IndexOf(panel_Old);
                if(index != -1)
                {
                    if (dictionary.ContainsKey(panel_Old))
                    {
                        dictionary[panel_Old] = new List<Geometry.Planar.ISegmentable2D>() { segment2D_New };
                        result.Add(panel_New.Guid);
                        panels[index] = panel_New;

                        index = externalPanels_New.IndexOf(panel_Old);
                        if(index != -1)
                        {
                            externalPanels_New[index] = panel_New;
                        }
                    }
                }
            }

            return result.ToList();
        }
    }
}