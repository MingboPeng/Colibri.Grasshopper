using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace ImageParameter
{
    public class ImageParameterComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ImageParameterComponent()
          : base("ImageParameter", "ImageParameter",
              "ImageParameter",
              "TT Toolbox", "Colibri")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("widthInput", "Width", "imageWidth", GH_ParamAccess.item);
            pManager.AddIntegerParameter("heightInput", "Height", "imageHeight", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("imageParameter", "ImgParameter", "imageParameter", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input variables
            int width = 100;
            int height = 100;
            
            

            //get data
            DA.GetData(0, ref width);
            DA.GetData(1, ref height);
            

            Dictionary<string, int> imageP = new Dictionary<string, int>();

            imageP.Add("Width", width);
            imageP.Add("Height", height);




            


            //set output
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
