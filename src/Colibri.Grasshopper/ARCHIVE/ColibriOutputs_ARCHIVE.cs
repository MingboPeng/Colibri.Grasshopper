using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Colibri.Grasshopper
{
    public class ColibriOutputs_ARCHIVE: GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// ne	w tabs/panels will automatically be created.
        /// </summary>
        public ColibriOutputs_ARCHIVE()
          : base("Colibri Outputs", "Colibri Outputs",
              "Collects design outputs (us engineers would call these 'performance metrics') to chart in Design Explorer.  These will be the vertical axes to the far right on the parallel coordinates plot, next to the design inputs.  These values should describe the characteristics of a single design iteration.",
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
            pManager.AddTextParameter("Names", "Names", "Output names.  These names will show up on top of vertical axes in Design Explorer's parallel coordinates plot.", GH_ParamAccess.list);
            pManager.AddTextParameter("Values", "Values", "Output Values.  This list should be the same length as the list of names.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Outputs", "Outputs", "Colibri's Outputs object.  Plug this into the Colibri aggregator downstream.", GH_ParamAccess.list);
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

            //defense
            if (OutputNames.Count != OutputValues.Count)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please provide equal numbers of Names and Values.");
                return;
            }

            //dict to populate
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();

            //loop over headings
            for (int i = 0; i < OutputNames.Count; i++)
            {
                try
                {
                    myDictionary.Add(OutputNames[i], OutputValues[i]);
                }
                catch (ArgumentException ex)
                {
                    if (ex.ToString().Contains("key"))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Your Outputs must have unique names!  Set them all and try again.");
                        return;
                    }
                    else
                    {
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
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