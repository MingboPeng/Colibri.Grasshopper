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
              "Colibri", "Colibri")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddTextParameter("widthInput", "w", "imageWidth", GH_ParamAccess.list);
            //pManager.AddTextParameter("heightInput", "h", "imageHeight", GH_ParamAccess.item);
            pManager.AddIntegerParameter("widthInput", "width", "imageWidth", GH_ParamAccess.item);
            pManager.AddIntegerParameter("heightInput", "height", "imageHeight", GH_ParamAccess.item);
            pManager.AddBooleanParameter("bool", "bool", "bool", GH_ParamAccess.item);
            //pManager.AddTextParameter("imgName", "name", "imgName", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("imageParameter", "imageParameter", "imageParameter", GH_ParamAccess.list);
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
            bool run = false;
            

            //get data
            DA.GetData(0, ref width);
            DA.GetData(1, ref height);
            DA.GetData(2, ref run);
            //operations
            //string widthOutput = Convert.ToString(width);
            //string heightOutput = Convert.ToString(height);

            //string parameterOutput = widthOutput + "_" + heightOutput;

            Dictionary<string, int> imageP = new Dictionary<string, int>();

            imageP.Add("Width", width);
            imageP.Add("Height", height);




            if (run)
               {

                System.Drawing.Size realSize = new System.Drawing.Size();
                realSize.Height = height;
                realSize.Width = width;
                //Rhino.Display.DisplayModeDescription currentDisplay = new Rhino.Display.DisplayModeDescription.GetDisplayMode;
                System.Drawing.Bitmap screenShot = new System.Drawing.Bitmap();
                screenShot = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.CaptureToBitmap(realSize);
                string filePath = @"F:\1209.png";
                System.Drawing.Bitmap.Save(screenShot, filePath);
               }
            


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
                return null;
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
