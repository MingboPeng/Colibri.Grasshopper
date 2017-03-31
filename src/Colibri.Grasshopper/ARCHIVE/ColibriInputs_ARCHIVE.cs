using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using GH = Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
using System.Linq;
using Grasshopper.Kernel.Special;

namespace Colibri.Grasshopper
{
    public class ColibriInputs_ARCHIVE : GH_Component
    {
        GH_Document doc = null;

        /// <summary>
        /// Initializes a new instance of the ColibriInputs class.
        /// </summary>
        public ColibriInputs_ARCHIVE()
          : base("Colibri Inputs", "Colibri Inputs",
              "Collects design input values to chart in Design Explorer.  These will be the vertical axes to the left on the parallel coordinates plot.  These values should describe the genome of a single design iteration.\n\nThis component lets you easily use Colibri to record a Galapagos or Octopus run, and to review all iterations in Design Explorer.",
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
            pManager.AddNumberParameter("Sliders", "Sliders", "Input sliders for Colibri to record.  These should be the same sliders that are being driven by Galapagos or Octopus.", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Inputs", "Inputs", "Colibri's Inputs object.  Plug this into the Colibri aggregator downstream.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //get a reference to the current doc the first time through
            if (doc == null)
            {
                doc = GH.Instances.ActiveCanvas.Document;
            }

            //catch grasshopper inputs
            List<double> sliderValues = new List<double>();
            DA.GetDataList(0, sliderValues);

            //get connected sliders
            List<GH_NumberSlider> connectedSliders = getConnectedSliders();

            //output 'inputs' object
            Dictionary<string, double> inputs = new Dictionary<string, double>();
            for (int i = 0; i < sliderValues.Count; i++)
            {
                try
                {
                    inputs.Add(connectedSliders[i].NickName, sliderValues[i]);
                }
                catch (ArgumentException ex)
                {
                    if (ex.ToString().Contains("key"))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Your sliders must have unique nicknames!  Set them all and try again.");
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
            DA.SetDataList(0, inputs);



        }

        private List<GH_NumberSlider> getConnectedSliders()
        {

            // Find the Guid for connected slides
            List<System.Guid> guids = new List<System.Guid>(); //empty list for guids
            GH.Kernel.IGH_Param selSlidersInput = this.Params.Input[0]; //ref for input where sliders are connected to this component
            IList<GH.Kernel.IGH_Param> sources = selSlidersInput.Sources; //list of things connected on this input
            bool isAnythingConnected = sources.Any(); //is there actually anything connected?

            // Find connected
            if (isAnythingConnected)
            { //if something's connected,
                foreach (var source in sources) //for each of these connected things:
                {
                    IGH_DocumentObject component = source.Attributes.GetTopLevel.DocObject; //for this connected thing, bring it into the code in a way where we can access its properties
                    GH.Kernel.Special.GH_NumberSlider mySlider = component as GH.Kernel.Special.GH_NumberSlider; //...then cast (?) it as a slider
                    if (mySlider == null) //of course, if the thing isn't a slider, the cast doesn't work, so we get null. let's filter out the nulls
                        continue;
                    guids.Add(mySlider.InstanceGuid); //things left over are sliders and are connected to our input. save this guid.
                                                      //we now have a list of guids of sliders connected to our input, saved in list var 'mySlider'
                }
            }

            // Find all sliders.
            List<GH.Kernel.Special.GH_NumberSlider> sliders = new List<GH.Kernel.Special.GH_NumberSlider>();
            foreach (IGH_DocumentObject docObject in doc.Objects)
            {
                GH.Kernel.Special.GH_NumberSlider slider = docObject as GH.Kernel.Special.GH_NumberSlider;
                if (slider != null)
                {
                    // check if the slider is in the selected list
                    if (isAnythingConnected)
                    {
                        if (guids.Contains(slider.InstanceGuid)) sliders.Add(slider);
                    }
                    else sliders.Add(slider);
                }
            }


            /*foreach (GH.Kernel.Special.GH_NumberSlider slider in sliders)
            {
                names.Add(slider.NickName);
            }*/

            return sliders;
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
                return Properties.Resources.Colibri_logobase_5;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{8875269a-634c-4c19-b196-f787a70d1b59}"); }
        }
    }
}