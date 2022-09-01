﻿using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Modify
    {
        public static List<Space> SplitSpace(this AdjacencyCluster adjacencyCluster, Guid spaceGuid, Func<Panel, double> func, double elevationOffset = 0.1, double minSectionOffset = 1, double silverSpacing = Core.Tolerance.MacroDistance, double tolerance_Angle = Core.Tolerance.Angle, double tolerance_Distance = Core.Tolerance.Distance, double tolerance_Snap = Core.Tolerance.MacroDistance)
        {
            if(adjacencyCluster == null || spaceGuid == Guid.Empty)
            {
                return null;
            }

            Space space = adjacencyCluster.GetObject<Space>(spaceGuid);
            if(space == null)
            {
                return null;
            }

            Shell shell = adjacencyCluster.Shell(space);
            if(shell == null)
            {
                return null;
            }

            BoundingBox3D boundingBox3D = shell.GetBoundingBox();

            Plane plane_Top = Geometry.Spatial.Create.Plane(boundingBox3D.Max.Z);

            Plane plane_Offset = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z + elevationOffset);

            Plane plane_Bottom = Geometry.Spatial.Create.Plane(boundingBox3D.Min.Z);

            List<Face3D> face3Ds = shell.Section(plane_Offset, true, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if(face3Ds == null || face3Ds.Count == 0)
            {
                return null;
            }

            List<Panel> panels = adjacencyCluster.GetPanels(space);
            if (panels == null || panels.Count == 0)
            {
                return null;
            }

            List<Face3D> face3Ds_New = new List<Face3D>();
            foreach (Face3D face3D in face3Ds)
            {
                Plane plane_Face3D = face3D.GetPlane();
                if(plane_Face3D == null)
                {
                    continue;
                }

                Face2D face2D = plane_Face3D.Convert(face3D);
                if(face2D == null)
                {
                    continue;
                }

                ISegmentable2D segmentable2D =  face2D.ExternalEdge2D as ISegmentable2D;
                if(segmentable2D == null)
                {
                    throw new NotImplementedException();
                }

                Polygon2D polygon2D = new Polygon2D(segmentable2D.GetPoints());

                List<double> offsets = new List<double>();
                foreach(Segment2D segment2D in polygon2D.GetSegments())
                {
                    Point3D point3D = plane_Face3D.Convert(segment2D.Mid());
                    if(point3D == null)
                    {
                        offsets.Add(0);
                        continue;
                    }

                    Panel panel = panels.Find(x => x.GetFace3D(false).On(point3D, tolerance_Snap));
                    if(panel == null)
                    {
                        offsets.Add(0);
                        continue;
                    }

                    double offset_Panel = func.Invoke(panel);
                    if(double.IsNaN(offset_Panel))
                    {
                        offsets.Add(0);
                        continue;
                    }

                    offsets.Add(offset_Panel);
                }

                //Offset polygon2D
                List<Polygon2D> polygon2Ds_Offset = polygon2D.Offset(offsets, true, true, true, tolerance_Distance);
                if(polygon2Ds_Offset == null || polygon2Ds_Offset.Count == 0)
                {
                    continue;
                }

                //Remove unwanted polygon2Ds
                for(int i = polygon2Ds_Offset.Count - 1; i >= 0; i--)
                {
                    Polygon2D polygon2D_Offset = polygon2Ds_Offset[i];

                    if (polygon2D_Offset == null || !polygon2D_Offset.IsValid())
                    {
                        polygon2Ds_Offset.RemoveAt(i);
                        continue;
                    }

                    List<Polygon2D> polygon2Ds_Temp = polygon2D_Offset.Offset(-minSectionOffset, tolerance_Distance);
                    if (polygon2Ds_Temp == null || polygon2Ds_Temp.Count == 0)
                    {
                        polygon2Ds_Offset.RemoveAt(i);
                        continue;
                    }

                }

                Func<Segment2D, Face3D> createFace3D = new Func<Segment2D, Face3D>((Segment2D segment2D) =>
                {
                    if(segment2D == null)
                    {
                        return null;
                    }

                    Segment3D segment3D = plane_Face3D.Convert(segment2D);
                    if(segment3D == null)
                    {
                        return null;
                    }


                    Segment3D segment3D_Top = plane_Top.Project(segment3D);
                    if(segment3D_Top == null || !segment3D_Top.IsValid() || segment3D_Top.GetLength() < tolerance_Snap)
                    {
                        return null;
                    }

                    Segment3D segment3D_Bottom = plane_Bottom.Project(segment3D);
                    if (segment3D_Bottom == null || !segment3D_Bottom.IsValid() || segment3D_Bottom.GetLength() < tolerance_Snap)
                    {
                        return null;
                    }

                    return new Face3D(new Polygon3D(new Point3D[] { segment3D_Bottom[0], segment3D_Bottom[1], segment3D_Top[1], segment3D_Top[0] }, tolerance_Distance));
                });
                
                //Create new face3Ds
                foreach(Polygon2D polygon2D_Offset in polygon2Ds_Offset)
                {
                    foreach (Segment2D segment2D in polygon2D_Offset.GetSegments())
                    {
                        if(segment2D == null)
                        {
                            continue;
                        }

                        if(segment2D.GetLength() < tolerance_Distance)
                        {
                            continue;
                        }

                        Point3D point3D = plane_Face3D.Convert(segment2D.Mid());
                        if (point3D == null)
                        {
                            continue;
                        }

                        Panel panel = panels.Find(x => x.GetFace3D(false).On(point3D, tolerance_Snap));
                        if (panel != null)
                        {
                            continue;
                        }

                        Face3D face3D_New = createFace3D.Invoke(segment2D);
                        if(face3D_New == null)
                        {
                            continue;
                        }

                        face3Ds_New.Add(face3D_New);
                    }
                }

                //Create additional new face3Ds
                foreach (Polygon2D polygon2D_Offset in polygon2Ds_Offset)
                {
                    List<Segment2D> segment2Ds = polygon2D_Offset.GetSegments();
                    if(segment2Ds == null || segment2Ds.Count == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < segment2Ds.Count; i++)
                    {
                        Segment2D segment2D_1 = segment2Ds[i];
                        Segment2D segment2D_2 = Core.Query.Next(segment2Ds, i);

                        Vector2D vector2D_1 = Geometry.Planar.Query.TraceFirst(segment2D_1[1], segment2D_1.Direction, polygon2D);
                        if (vector2D_1 == null)
                        {
                            continue;
                        }

                        Vector2D vector2D_2 = Geometry.Planar.Query.TraceFirst(segment2D_2[0], segment2D_2.Direction.GetNegated(), polygon2D);
                        if (vector2D_2 == null)
                        {
                            continue;
                        }

                        Point2D point2D_1 = segment2D_1[1].GetMoved(vector2D_1);
                        Point2D point2D_2 = segment2D_2[0].GetMoved(vector2D_2);

                        Point2D point2D = null;
                        double distance = double.MaxValue;
                        foreach (Point2D point2D_Polygon2D in polygon2D)
                        {
                            double distance_Temp = point2D_1.Distance(point2D_Polygon2D) + point2D_2.Distance(point2D_Polygon2D);
                            if (distance_Temp < distance)
                            {
                                point2D = point2D_Polygon2D;
                                distance = distance_Temp;
                            }
                        }

                        if (point2D == null)
                        {
                            continue;
                        }

                        Segment2D segment2D = new Segment2D(segment2D_1[1], point2D);
                        if (segment2D == null || !segment2D.IsValid() || segment2D.GetLength() < tolerance_Snap)
                        {
                            continue;
                        }

                        if (polygon2D.On(segment2D.Mid(), tolerance_Snap))
                        {
                            continue;
                        }

                        Face3D face3D_New = createFace3D.Invoke(segment2D);
                        if (face3D_New == null)
                        {
                            continue;
                        }

                        face3Ds_New.Add(face3D_New);

                    }

                    //foreach(Point2D point2D in polygon2D_Offset.GetPoints())
                    //{
                    //    int index = polygon2D.IndexOfClosestPoint2D(point2D);
                    //    if(index == -1)
                    //    {
                    //        continue;
                    //    }

                    //    Point2D point2D_Polygon = polygon2D.Points[index];
                    //    if(point2D_Polygon.AlmostEquals(point2D, tolerance_Snap))
                    //    {
                    //        continue;
                    //    }

                    //    if(polygon2D.On(point2D_Polygon.Mid(point2D), tolerance_Snap))
                    //    {
                    //        continue;
                    //    }

                    //    Segment2D segment2D = new Segment2D(point2D, point2D_Polygon);

                    //    Face3D face3D_New = createFace3D.Invoke(segment2D);
                    //    if (face3D_New == null)
                    //    {
                    //        continue;
                    //    }

                    //    face3Ds_New.Add(face3D_New);
                    //}
                }

            }

            if(face3Ds_New == null || face3Ds_New.Count == 0)
            {
                return null;
            }

            List<Panel> panels_Result = adjacencyCluster.AddPanels(face3Ds_New, null, new Space[] { space }, silverSpacing, tolerance_Angle, tolerance_Distance, tolerance_Snap);
            if(panels_Result == null)
            {
                return null;
            }

            List<Space> result = new List<Space>();
            foreach(Panel panel_Result in panels_Result)
            {
                List<Space> spaces_Panel = adjacencyCluster.GetRelatedObjects<Space>(panel_Result);
                if(spaces_Panel == null)
                {
                    continue;
                }

                foreach(Space space_Panel in spaces_Panel)
                {
                    if(result.Find(x => x.Guid == space_Panel.Guid) != null)
                    {
                        continue;
                    }

                    result.Add(space_Panel);
                }
            }

            return result;
        }
    }
}