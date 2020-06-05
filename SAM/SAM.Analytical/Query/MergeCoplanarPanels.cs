﻿using NetTopologySuite.Geometries;
using SAM.Geometry;
using SAM.Geometry.Planar;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static List<Panel> MergeCoplanarPanels(this IEnumerable<Panel> panels, double offset, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            List<Panel> redundantPanels = new List<Panel>();
            return MergeCoplanarPanels(panels, offset, ref redundantPanels, minArea, tolerance);
        }

        public static List<Panel> MergeCoplanarPanels(this IEnumerable<Panel> panels, double offset, ref List<Panel> redundantPanels, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            Dictionary<PanelGroup, List<Panel>> dictionary = new Dictionary<PanelGroup, List<Panel>>();
            foreach (PanelGroup panelGroup in Enum.GetValues(typeof(PanelGroup)))
                dictionary[panelGroup] = new List<Panel>();

            foreach (Panel panel in panels)
            {
                if (panel == null)
                    continue;

                dictionary[PanelGroup(panel.PanelType)].Add(panel);
            }

            List<Panel> result = new List<Panel>();

            foreach (PanelGroup panelGroup in Enum.GetValues(typeof(PanelGroup)))
            {
                List<Panel> panels_Temp = MergeCoplanarPanels(dictionary[panelGroup], offset, ref redundantPanels, minArea, tolerance);
                if (panels_Temp != null && panels_Temp.Count > 0)
                    result.AddRange(panels_Temp);
            }

            return result;
        }

        private static List<Panel> MergeCoplanarPanels(this List<Panel> panels, double offset, ref List<Panel> redundantPanels, double minArea = Core.Tolerance.MacroDistance, double tolerance = Core.Tolerance.Distance)
        {
            if (panels == null)
                return null;

            List<Panel> panels_Temp = panels.ToList();

            panels_Temp.Sort((x, y) => y.GetArea().CompareTo(x.GetArea()));

            List<Panel> result = new List<Panel>(panels);
            HashSet<Guid> guids = new HashSet<Guid>();

            while (panels_Temp.Count > 0)
            {
                Panel panel = panels_Temp[0];
                panels_Temp.RemoveAt(0);

                Plane plane = panel.PlanarBoundary3D?.Plane;
                if (plane == null)
                    continue;

                List<Panel> panels_Offset = new List<Panel>();
                foreach (Panel panel_Temp in panels_Temp)
                {
                    if (!panel_Temp.Construction.Name.Equals(panel.Construction?.Name))
                        continue;

                    Plane plane_Temp = panel_Temp?.PlanarBoundary3D?.Plane;
                    if (plane == null)
                        continue;

                    if (!plane.Coplanar(plane_Temp))
                        continue;

                    double distance = plane.Distance(plane_Temp);
                    if (distance > offset)
                        continue;

                    panels_Offset.Add(panel_Temp);
                }

                if (panels_Offset == null || panels_Offset.Count == 0)
                    continue;

                panels_Offset.Add(panel);

                List<Tuple<Polygon, Panel>> tuples_Polygon = new List<Tuple<Polygon, Panel>>();
                foreach (Panel panel_Temp in panels_Offset)
                {
                    Face2D face2D = plane.Convert(plane.Project(panel_Temp.GetFace3D()));

                    tuples_Polygon.Add(new Tuple<Polygon, Panel>(face2D.ToNTS(), panel_Temp));
                }

                List<Polygon> polygons_Temp = tuples_Polygon.ConvertAll(x => x.Item1);
                Geometry.Planar.Modify.RemoveAlmostSimilar_NTS(polygons_Temp, tolerance);

                polygons_Temp = Geometry.Planar.Query.Union(polygons_Temp);
                foreach (Polygon polygon in polygons_Temp)
                {
                    if (polygon.Area < minArea)
                        continue;

                    List<Tuple<Polygon, Panel>> tuples_Panel = tuples_Polygon.FindAll(x => polygon.Contains(x.Item1.InteriorPoint));
                    if (tuples_Panel == null || tuples_Panel.Count == 0)
                        continue;

                    tuples_Panel.Sort((x, y) => y.Item1.Area.CompareTo(x.Item1.Area));

                    foreach (Tuple<Polygon, Panel> tuple in tuples_Panel)
                    {
                        result.Remove(tuple.Item2);
                        panels_Temp.Remove(tuple.Item2);
                    }

                    Panel panel_Old = tuples_Panel.First().Item2;
                    tuples_Panel.RemoveAt(0);
                    redundantPanels.AddRange(tuples_Panel.ConvertAll(x => x.Item2));

                    if (panel_Old == null)
                        continue;

                    Polygon polygon_Temp = Geometry.Planar.Query.SimplifyByNTS_Snapper(polygon, tolerance);
                    polygon_Temp = Geometry.Planar.Query.SimplifyByNTS_TopologyPreservingSimplifier(polygon_Temp, tolerance);

                    Face2D face2D = polygon_Temp.ToSAM(minArea, Core.Tolerance.MicroDistance);
                    if (face2D == null)
                        continue;

                    Face3D face3D = new Face3D(plane, face2D);
                    Guid guid = panel_Old.Guid;
                    if (guids.Contains(guid))
                        guid = Guid.NewGuid();

                    Panel panel_New = new Panel(guid, panel_Old, face3D);

                    result.Add(panel_New);
                }
            }

            return result;
        }
    }
}