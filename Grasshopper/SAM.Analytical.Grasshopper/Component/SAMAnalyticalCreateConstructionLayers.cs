﻿using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateConstructionLayers : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("53f53e15-d92b-4515-b94d-0fa6fed4a785");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateConstructionLayers()
          : base("SAMAnalytical.CreateConstructionLayers", "SAMAnalyticalCreate.ConstructionLayers",
              "Create Construction Layers",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {           
            inputParamManager.AddTextParameter("_names", "_names", "Contruction Layer Name", GH_ParamAccess.list);
            inputParamManager.AddNumberParameter("_thicknesses", "_thicknesses", "Contruction Layer Thicknesses", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooConstructionLayerParam(), "ConstructionLayers", "ConstructionLayers", "SAM Analytical Construction Layers", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            List<string> names = new List<string>();
            if (!dataAccess.GetDataList(0, names))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<double> thicknesses = new List<double>();
            if (!dataAccess.GetDataList(1, thicknesses))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            object[] objects = new object[names.Count + thicknesses.Count];
            for (int i = 0; i < names.Count; i++)
                objects[i] = names[i];

            for (int i = 0; i < thicknesses.Count; i++)
                objects[names.Count + i] = thicknesses[i];

            List<ConstructionLayer> constructionLayers = Create.ConstructionLayers(objects);

            dataAccess.SetDataList(0, constructionLayers?.ConvertAll(x => new GooConstructionLayer(x)));
        }
    }
}