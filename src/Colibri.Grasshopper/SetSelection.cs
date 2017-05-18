using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Colibri.Grasshopper
{
    
    public class SetSelection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IteratorSelection class.
        /// </summary>
        public SetSelection()
          : base("Iteration Selection", "Sel",
              "Generates iteration selections for Iterator.",
              "TT Toolbox", "Colibri 2.0")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Divisions", "Divisions", "Numbers to take on each Iterator input. This should be a list of integers of the same length as the list of Iterator inputs. By default, iterator will go through each smallest step as possible.\n\n 0: for all values \n 1: for current position \n >1: numbers to be evenly picked.", GH_ParamAccess.list);
            pManager[0].Optional = true;

            pManager.AddIntervalParameter("Domains", "Domains", "Set the ranges of all iteration combinations, can be one or a list of 1d domains (use Construct Domain). \n\nDomain setting comes after Divisions setting.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Selection", "Selection", "Selections for Iterator", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var divisions = new List<int>();
            var domains = new List<GH_Interval>();

            //get Data
            DA.GetDataList(0, divisions);
            DA.GetDataList(1, domains);

            foreach (var item in domains)
            {
                if (item.Value.Min<0 ||item.Value.Max ==0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Domain should within or equal (min:0 TO max:total)");
                    return;
                }

            }

            foreach (var item in divisions)
            {
                if (item < 0 )
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Divisions should be larger or equal 0");
                    return;
                }

            }

            if (RuntimeMessageLevel != GH_RuntimeMessageLevel.Error)
            {
                var selections = new IteratorSelection(divisions, domains);

                //set Data
                DA.SetData(0, selections);
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Properties.Resources.Selection;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7dab01df-60be-477a-83d0-ff37a89d6a5a}"); }
        }
    }
}