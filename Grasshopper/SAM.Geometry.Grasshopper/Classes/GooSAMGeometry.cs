﻿using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Newtonsoft.Json.Linq;
using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using SAM.Geometry.Grasshopper.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SAM.Geometry.Grasshopper
{
    public class GooSAMGeometry : GH_Goo<ISAMGeometry>, IGH_PreviewData, IGH_BakeAwareData
    {
        public GooSAMGeometry()
            : base()
        {
        }

        public GooSAMGeometry(ISAMGeometry sAMGeometry)
        {
            Value = sAMGeometry;
        }

        public override bool IsValid => Value != null;

        public override string TypeName
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(ISAMGeometry);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.Name;
            }
        }

        public override string TypeDescription
        {
            get
            {
                Type type = null;

                if (Value == null)
                    type = typeof(ISAMGeometry);
                else
                    type = Value.GetType();

                if (type == null)
                    return null;

                return type.FullName.Replace(".", " ");
            }
        }

        public virtual BoundingBox ClippingBox
        {
            get
            {
                if (Value is Spatial.IBoundable3D)
                    return Rhino.Convert.ToRhino(((Spatial.IBoundable3D)Value).GetBoundingBox());

                if (Value is Spatial.Point3D)
                    return Rhino.Convert.ToRhino(((Spatial.Point3D)(object)Value).GetBoundingBox(1));

                if (Value is Planar.IBoundable2D)
                {
                    Planar.BoundingBox2D boundingBox2D = ((Planar.IBoundable2D)Value).GetBoundingBox();
                    return Rhino.Convert.ToRhino( new Spatial.BoundingBox3D(new Spatial.Point3D(boundingBox2D.Min.X, boundingBox2D.Min.Y, -1), new Spatial.Point3D(boundingBox2D.Max.X, boundingBox2D.Max.Y, 1)));
                }

                if (Value is Planar.Point2D)
                {
                    Planar.Point2D point2D = (Planar.Point2D)Value;
                    return Rhino.Convert.ToRhino(new Spatial.BoundingBox3D(new Spatial.Point3D(point2D.X, point2D.Y, -1), new Spatial.Point3D(point2D.X, point2D.Y, 1)));
                }

                if (Value is Spatial.Plane)
                {
                    return Rhino.Convert.ToRhino(((Spatial.Plane)Value).Origin.GetBoundingBox(1));
                }

                return new BoundingBox();
            }
        }

        public override IGH_Goo Duplicate()
        {
            return new GooSAMGeometry(Value);
        }

        public override bool Write(GH_IWriter writer)
        {
            if (Value == null)
                return false;

            JObject jObject = Value.ToJObject();
            if (jObject == null)
                return false;

            writer.SetString(typeof(ISAMGeometry).FullName, jObject.ToString());
            return true;
        }

        public override bool Read(GH_IReader reader)
        {
            string value = null;
            if (!reader.TryGetString(typeof(ISAMGeometry).FullName, ref value))
                return false;

            if (string.IsNullOrWhiteSpace(value))
                return false;

            Value = Core.Create.IJSAMObject<ISAMGeometry>(value);
            return true;
        }

        public override string ToString()
        {
            if (Value == null)
                return typeof(ISAMGeometry).Name;

            return Value.GetType().Name;
        }

        public override bool CastFrom(object source)
        {
            if (source is ISAMGeometry)
            {
                Value = (ISAMGeometry)source;
                return true;
            }

            if (typeof(IGH_Goo).IsAssignableFrom(source.GetType()))
            {
                try
                {
                    source = (source as dynamic).Value;
                }
                catch
                {
                }

                if (source is ISAMGeometry)
                {
                    Value = (ISAMGeometry)source;
                    return true;
                }
            }

            if (source is Polyline)
            {
                Value = Rhino.Convert.ToSAM((Polyline)source);
                return true;
            }

            if (source is Point3d)
            {
                Value = Rhino.Convert.ToSAM((Point3d)source);
                return true;
            }

            if(source is Rectangle3d)
            {
                Value = Rhino.Convert.ToSAM((Rectangle3d)source);
                return true;
            }

            if (source is GH_Curve)
            {
                Value = Convert.ToSAM((GH_Curve)source);
                return true;
            }

            if (source is GH_Point)
            {
                Value = Convert.ToSAM((GH_Point)source);
                return true;
            }

            if (source is GH_Rectangle)
            {
                Value = Convert.ToSAM((GH_Rectangle)source);
                return true;
            }

            if (source is GH_Vector)
            {
                Value = Convert.ToSAM((GH_Vector)source);
                return true;
            }

            if (source is Vector3d)
            {
                Value = Rhino.Convert.ToSAM((Vector3d)source);
                return true;
            }

            if (source is GH_Plane)
            {
                Value = Convert.ToSAM((GH_Plane)source);
                return true;
            }

            if (source is Plane)
            {
                Value = Rhino.Convert.ToSAM((Plane)source);
                return true;
            }

            if(source is Mesh)
            {
                Value = Rhino.Convert.ToSAM((Mesh)source);
            }

            if (source is GH_Mesh)
            {
                Value = Convert.ToSAM((GH_Mesh)source);
            }

            if (source is Brep)
            {
                List<Spatial.ISAMGeometry3D> sAMGeometry3Ds = Rhino.Convert.ToSAM(((Brep)source));
                if(sAMGeometry3Ds != null && sAMGeometry3Ds.Count != 0)
                {
                    Value = sAMGeometry3Ds[0];
                    return true;
                }
            }

            return base.CastFrom(source);
        }

        public override bool CastTo<Y>(ref Y target)
        {
            if (typeof(Y) is ISAMGeometry)
            {
                target = (Y)(object)Value;
                return true;
            }

            if (typeof(Y) == typeof(Polyline))
            {
                if (Value is Spatial.ISegmentable3D)
                {
                    target = (Y)(object)(new Polyline(((Spatial.ISegmentable3D)Value).GetPoints().ConvertAll(x => Rhino.Convert.ToRhino(x))));
                    return true;
                }
            }

            if (typeof(Y) == typeof(Point3d))
            {
                if (Value is Spatial.Point3D)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((((Spatial.Point3D)Value)));
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Point))
            {
                if (Value is Spatial.Point3D)
                {
                    target = (Y)(object)(((Spatial.Point3D)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Plane))
            {
                if (Value is Spatial.Plane)
                {
                    target = (Y)(object)(((Spatial.Plane)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Rectangle))
            {
                if (Value is Spatial.Rectangle3D)
                {
                    target = (Y)(object)(((Spatial.Rectangle3D)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y) == typeof(Plane))
            {
                if (Value is Spatial.Plane)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((Spatial.Plane)Value);
                    return true;
                }
            }

            if (typeof(Y) == typeof(Vector3d))
            {
                if (Value is Spatial.Vector3D)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((Spatial.Vector3D)Value);
                    return true;
                }

                if (Value is Planar.Vector2D)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((Planar.Vector2D)Value);
                    return true;
                }
            }

            if(typeof(Y) == typeof(Rectangle3d))
            {
                if(Value is Spatial.Rectangle3D)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((Spatial.Rectangle3D)Value);
                    return true;
                }

                if (Value is Planar.Rectangle2D)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((Planar.Rectangle2D)Value);
                    return true;
                }
            }

            if (typeof(Y) == typeof(GH_Vector))
            {
                if (Value is Spatial.Vector3D)
                {
                    target = (Y)(object)(((Spatial.Vector3D)Value).ToGrasshopper());
                    return true;
                }

                if (Value is Planar.Vector2D)
                {
                    target = (Y)(object)(((Planar.Vector2D)Value).ToGrasshopper());
                    return true;
                }
            }

            if (typeof(Y).IsAssignableFrom(typeof(Brep)))
            {
                if (Value is Spatial.Shell)
                {
                    target = (Y)(object)Rhino.Convert.ToRhino((Spatial.Shell)Value);
                    return true;
                }
            }

            if (typeof(Y).IsAssignableFrom(typeof(GH_Brep)))
            {
                if (Value is Spatial.Shell)
                {
                    target = (Y)(object)new GH_Brep(Rhino.Convert.ToRhino((Spatial.Shell)Value));
                    return true;
                }
            }

            if (typeof(Y).IsAssignableFrom(typeof(GH_Mesh)))
            {
                if (Value is Spatial.Shell)
                {
                    Mesh mesh = Rhino.Convert.ToRhino_Mesh((Spatial.Shell)Value);
                    if(mesh != null)
                    {
                        target = (Y)(object)new GH_Mesh(mesh);
                        return true;
                    }
                }

                if(Value is Spatial.Mesh3D)
                {
                    target = (Y)(object)new GH_Mesh(Rhino.Convert.ToRhino((Spatial.Mesh3D)Value));
                    return true;
                }
            }

            if (typeof(Y).IsAssignableFrom(Value.GetType()))
            {
                target = (Y)(object)Value;
                return true;
            }

            return base.CastTo(ref target);
        }

        public virtual void DrawViewportWires(GH_PreviewWireArgs args)
        {
            DrawViewportWires(args, args.Color);
        }

        public virtual void DrawViewportWires(GH_PreviewWireArgs args, System.Drawing.Color color)
        {
            Modify.DrawViewportWires(Value, args, color);
        }

        public virtual void DrawViewportMeshes(GH_PreviewMeshArgs args)
        {
            Modify.DrawViewportMeshes(Value, args, args.Material);
        }

        public virtual void DrawViewportMeshes(GH_PreviewMeshArgs args, global::Rhino.Display.DisplayMaterial displayMaterial)
        {
            Modify.DrawViewportMeshes(Value, args, displayMaterial);
        }

        public bool BakeGeometry(RhinoDoc doc, ObjectAttributes att, out Guid obj_guid)
        {
            return Rhino.Modify.BakeGeometry(Value, doc, att, out obj_guid);
        }
    }

    public class GooSAMGeometryParam : GH_PersistentParam<GooSAMGeometry>, IGH_BakeAwareObject, IGH_PreviewObject
    {
        public override Guid ComponentGuid => new Guid("b4f8eee5-8d45-4c52-b966-1be5efa7c1e6");

        protected override System.Drawing.Bitmap Icon => Resources.SAM_Geometry;

        bool IGH_PreviewObject.Hidden { get; set; }

        bool IGH_PreviewObject.IsPreviewCapable => !VolatileData.IsEmpty;

        BoundingBox IGH_PreviewObject.ClippingBox => Preview_ComputeClippingBox();

        public bool IsBakeCapable => true;

        void IGH_PreviewObject.DrawViewportMeshes(IGH_PreviewArgs args) => Preview_DrawMeshes(args);

        void IGH_PreviewObject.DrawViewportWires(IGH_PreviewArgs args) => Preview_DrawWires(args);

        public GooSAMGeometryParam()
            : base(typeof(ISAMGeometry).Name, typeof(ISAMGeometry).Name, typeof(ISAMGeometry).FullName.Replace(".", " "), "Params", "SAM")
        {
        }

        protected override GH_GetterResult Prompt_Plural(ref List<GooSAMGeometry> values)
        {
            throw new NotImplementedException();
        }

        protected override GH_GetterResult Prompt_Singular(ref GooSAMGeometry value)
        {
            throw new NotImplementedException();
        }

        public void BakeGeometry(RhinoDoc doc, List<Guid> obj_ids)
        {
            BakeGeometry(doc, doc.CreateDefaultAttributes(), obj_ids);
        }

        public void BakeGeometry(RhinoDoc doc, ObjectAttributes att, List<Guid> obj_ids)
        {
            foreach (var value in VolatileData.AllData(true))
            {
                Guid uuid = default;
                (value as IGH_BakeAwareData)?.BakeGeometry(doc, att, out uuid);
                obj_ids.Add(uuid);
            }
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Save As...", Menu_SaveAs, VolatileData.AllData(true).Any());

            //Menu_AppendSeparator(menu);

            base.AppendAdditionalMenuItems(menu);
        }

        private void Menu_SaveAs(object sender, EventArgs e)
        {
            Core.Grasshopper.Query.SaveAs(VolatileData);
        }
    }
}