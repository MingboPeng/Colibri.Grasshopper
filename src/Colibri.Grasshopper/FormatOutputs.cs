using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Colibri.Grasshopper
{
    public class FormatOutputs : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// ne	w tabs/panels will automatically be created.
        /// </summary>
        public FormatOutputs()
          : base("FormatOutputs", "FormatOutputs",
              "Formats Data Dictionary",
              "Colibri", "Colibri")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("OutputNames", "OutputNames", "Names of Outputs", GH_ParamAccess.list);
            pManager.AddTextParameter("OutputValues", "OutputValues", "Output Values", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Outputs", "Outputs", "Dictionary of Outputs", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Declare variables
            List<string> OutputNames = new List<string>();
            List<string> OutputValues = new List<string>();

            //catch inputs from Grasshopper

            DA.GetDataList(0, OutputNames);
            DA.GetDataList(1, OutputValues);

            //dict to populate
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();

            //loop over headings
            for (int i = 0; i < OutputNames.Count; i++)
            {
                myDictionary.Add(OutputNames[i], OutputValues[i]);
            }


            //set output data
            DA.SetDataList(0, myDictionary);

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
                return Properties.Resources.Colibri_logobase_2;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5cac6c29-d015-4489-b592-48ff52e8c33e}"); }
        }
    }
}