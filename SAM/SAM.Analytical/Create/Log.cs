﻿using SAM.Core;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Create
    {
        public static Log Log(this AdjacencyCluster adjacencyCluster)
        {
            if (adjacencyCluster == null)
                return null;

            Log result = new Log();

            List<Space> spaces = adjacencyCluster.GetSpaces();
            if(spaces == null || spaces.Count == 0)
            {
                result.Add("AdjacencyCluster has no spaces.", LogRecordType.Warning);
                return result;
            }

            foreach(Space space in spaces)
            {
                List<Panel> panels = adjacencyCluster.GetPanels(space);
                if(panels == null || panels.Count == 0)
                {
                    result.Add("Space {0} (Guid: {1}) is not enclosed.", LogRecordType.Warning, space.Name, space.Guid);
                    return result;
                }

                foreach(Panel panel in panels)
                {
                    if (panel == null)
                        continue;

                    if(panel.PanelType == PanelType.Shade || panel.PanelType == PanelType.SolarPanel || panel.PanelType == PanelType.Undefined)
                    {
                        result.Add("Panel {0} (Guid: {1}) has invalid PanelType assigned.", LogRecordType.Warning, panel.Name, panel.Guid);
                        return result;
                    }
                }
            }

            return result;
        }
    }
}