using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Colibri.Grasshopper
{
    public class ImageParameters_ARCHIVE : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ImageParameters_ARCHIVE()
          : base("Image Parameters", "Image Parameters",
              "Defines how Colibri generates images.  Right now this just sets the size, but we could expose more options like Ladybug's Capture View component.",
              "TT Toolbox", "Colibri")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.hidden; } }

        public override bool Obsolete { get { return true; } }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Width", "Width", "Image width in pixels.", GH_ParamAccess.item, 400);
            pManager.AddIntegerParameter("Height", "Height", "Image height in pixels.", GH_ParamAccess.item, 400);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ImgParams", "ImgParams", "Colibri's image parameters object.  Feed this into the Colibri aggregator downstream.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input variables
            int width = 400;
            int height = 400;
            
            //get data
            DA.GetData(0, ref width);
            DA.GetData(1, ref height);

            //defense
            if (width < 50 || height < 50)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Width and Height must be greater than 50 pixels.");
                return;
            }

            //set output
            Dictionary<string, int> imageP = new Dictionary<string, int>();
            imageP.Add("Width", width);
            imageP.Add("Height", height);
            
            DA.SetDataList(0, imageP);
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
                return Colibri.Grasshopper.Properties.Resources.Colibri_logobase_3;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{787196c8-5cc8-46f5-b253-4e63d8d271e0}"); }
        }
    }
}
