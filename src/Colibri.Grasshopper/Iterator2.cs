using System;
using System.Collections.Generic;
using GH = Grasshopper;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;

namespace Colibri.Grasshopper
{
    public class Iterator2 : GH_Component
    {
        
        GH_Document doc = null;
        public List<GH_NumberSlider> allConnectedSliders = new List<GH_NumberSlider>();

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Iterator2()
          : base("Iterator2", "Iterator",
              "Description",
              "Colibri", "Colibri")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("inputs", "inputs", "Slider or Panel", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Fly?", "Fly?", "Tell Colibri to fly!  Provide a button here, and click it once you are ready for Colibri to fly around your definition.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("input", "input", "current item of inputs", GH_ParamAccess.item);
            pManager.AddGenericParameter("Inputs", "Inputs", "connet to Aggregateor", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            if (doc == null)
            {
                doc = GH.Instances.ActiveCanvas.Document;
            }

            //List<string> inputs = new List<string>();
            //DA.GetDataList(0, inputs);

            var sliderAndPanel = new IteratorGetSliderPanel();
            var selectedSliderAndPanel = sliderAndPanel.getConnectedSliderOrPanel(this.Params.Input[0],this.Params.Output[0]);
            
            if (selectedSliderAndPanel.Any())
            {
                //IGH_Param newParam = Params.Input[2];
                //newParam.Name = "inputs";
                //newParam.NickName = "inputs";
                //newParam.Access = GH_ParamAccess.list;
                //http://www.grasshopper3d.com/forum/topics/dynamic-outputs-for-component

            //not working
               // GH_Component.GH_InputParamManager newParamManager = null;
               // newParamManager.AddGenericParameter("inputs", "inputs", "Slider or Panel", GH_ParamAccess.list);
               // this.RegisterInputParams(newParamManager);
                


            }
            


            //assign to output
            DA.SetDataList(0, selectedSliderAndPanel);

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
            get { return new Guid("{74a79561-b3b2-4e12-beb4-d79ec0ed378a}"); }
        }
    }
}