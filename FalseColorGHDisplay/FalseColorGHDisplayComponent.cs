using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using System.Drawing;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace FalseColorGHDisplay
{
    public class FalseColorGHDisplayComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FalseColorGHDisplayComponent()
          : base("FalseColorGHDisplay", "FalseColor",
              "Example of overriding the preview of Brep Objects",
              "FalseColor", "Components")
        {
        }

        List<Mesh> Meshes { get; set; }

        private BoundingBox _clippingBox;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "B", "Breps to draw", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Meshes = new List<Mesh>();
            var brepObjs = new List<GH_Brep>();

            _clippingBox = BoundingBox.Empty;

            if (DA.GetDataList(0, brepObjs))
            {

                foreach (var obj in brepObjs)
                {
                    
                    var mesh = new Mesh();
                    foreach (var m in Mesh.CreateFromBrep(obj.Value, MeshingParameters.Smooth))
                        mesh.Append(m);

                    mesh.Normals.ComputeNormals();
                    mesh.Compact();

                     var bb = mesh.GetBoundingBox(false);

                    _clippingBox.Union(bb);

                    Meshes.Add(mesh);
                }

                var bbMax = _clippingBox.Max.Z;
                var bbMin = _clippingBox.Min.Z;

                foreach (var m in Meshes)
                {
                    m.VertexColors.Clear();
                    for (int i = 0; i < m.Vertices.Count; i++)
                    {
                        var v = m.Vertices[i];
                        var val = Remap(v.Z, bbMin, bbMax, 0, 1);

                        var color = Color.FromArgb((int)(255 * val), (int)(255 * (1 - val)), 0);

                        m.VertexColors.Add(color);

                    }
                }

            }

        }

        public static double Remap(double value, double originStart, double originEnd, double targetStart, double targetEnd)
        {
            return ((value - originStart) / (originEnd - originStart) * (targetEnd - targetStart) + targetStart);
        }

        public override BoundingBox ClippingBox
        {
            get
            {
                BoundingBox box = base.ClippingBox;
                box.Union(_clippingBox);
                return box;
            }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            if (Hidden) return;
            if (Locked) return;

            base.DrawViewportMeshes(args);
            if (null != Meshes)
                foreach (var m in Meshes)
                    args.Display.DrawMeshFalseColors(m);
        }

        public override void ClearData()
        {
            base.ClearData();
          
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("145388dc-6de7-4c64-bfb5-b882f98a25ae"); }
        }
    }
}
