using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace Colibri.Grasshopper
{
    public class IteratorSelection
    {
        public bool IsDefined { get; private set; }
        public List<int>Steps { get; private set; }
        public List<GH_Interval> Domains { get; private set; }
        public int SelectedCounts { get; private set; }

        //Construction
        public IteratorSelection()
        {
            this.IsDefined = false;
        }
        public IteratorSelection(List<int> Steps, List<GH_Interval> Domains)
        {
            this.IsDefined = true;
            this.Steps = Steps;
            this.Domains = Domains;
            //this.SelectedCounts = calSelectedCounts(Steps, Domains);
        }

        //Method
        //private int calSelectedCounts(List<int> Steps, List<GH_Interval> Domains)
        //{
        //    int totalIterations = 1;

        //    foreach (var item in Steps)
        //    {
        //        int totalCount = item.TotalCount;
        //        if (totalCount > 0)
        //        {
        //            totalIterations *= totalCount;
        //        }
        //    }
        //    return 0;
        //}

        //override
        public override string ToString()
        {
            if (!IsDefined)
            {
                return null;
            }

            string outString="";
            if (Steps.Count!=0)
            {
                outString += "Take:" + Steps.Count+"\n";
            }

            if (Domains.Count != 0)
            {
                outString += "\nDomain:" + Domains.Count + "\n"; ;
            }

            return outString;

        }

    }

    public class SetSelection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the IteratorSelection class.
        /// </summary>
        public SetSelection()
          : base("Fly Selection", "Sel",
              "Generates iteration selections for Iterator.",
              "TT Toolbox", "Colibri")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Take", "Take", "Number of steps to TAKE on each Slider, ValueList or Panel.  This should be a list of integers (each of which must be greater than one) of the same length as the list of sliders plugged into the Sliders input.\n\nIf no input data is provided, we'll use every tick on every slider as a step.", GH_ParamAccess.list);
            pManager[0].Optional = true;

            pManager.AddIntervalParameter("Domains", "Domains", "Ranges for the Iterator selection, can be one or a list of 1d domains (use Construct Domain).", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Selections", "Sel", "Selections for Iterator", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var steps = new List<int>();
            var ranges = new List<GH_Interval>();

            //get Data
            DA.GetDataList(0, steps);
            DA.GetDataList(1, ranges);

            foreach (var item in ranges)
            {
                if (item.Value.Min<0)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Selection domain should starts from 0!");
                    return;
                }
            }
            var selections = new IteratorSelection(steps, ranges);

            //set Data
            DA.SetData(0, selections);
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
                return null;
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