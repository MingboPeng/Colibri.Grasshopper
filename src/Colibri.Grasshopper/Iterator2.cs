using System;
using System.Collections.Generic;
using GH = Grasshopper;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Components;

namespace Colibri.Grasshopper
{
    public class Iterator2 : GH_Component, IGH_VariableParameterComponent
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
            pManager.AddGenericParameter("Input", "A", "Please connect a Slider, Panel, or ValueList", GH_ParamAccess.list);
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

            
            //if (selectedSliderAndPanel.Any())
            //{
            //    IGH_Param newParam = null;
            //    newParam.Name = "inputs";
            //    newParam.NickName = "inputs";
            //    newParam.Access = GH_ParamAccess.list;
            //    this.Params.RegisterOutputParam(newParam, this.Params.Output.Count - 1);


            //    //http://www.grasshopper3d.com/forum/topics/dynamic-outputs-for-component

            //    //    //not working
            //    //    // GH_Component.GH_InputParamManager newParamManager = null;
            //    //    // newParamManager.AddGenericParameter("inputs", "inputs", "Slider or Panel", GH_ParamAccess.list);
            //    //    // this.RegisterInputParams(newParamManager);




            //}



            //assign to output
            //DA.SetDataList(0, selectedSliderAndPanel);

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




        #region Methods of IGH_VariableParameterComponent interface
        bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
        {
            bool isInputSide = (side == GH_ParameterSide.Input) ? true : false;
            bool isTheFlyButton = (index == this.Params.Input.Count) ? true : false;
            bool isTheOnlyInput = (index == 0) ? true : false;

            //We only let input parameters to be added (output number is fixed at one)
            if (isInputSide && !isTheFlyButton && !isTheOnlyInput)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        
        bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
        {
            bool isInputSide = (side == GH_ParameterSide.Input)? true : false;
            bool isTheFlyButton = (index == this.Params.IndexOfInputParam("Fly?")) ? true : false;
            bool isTheOnlyInput = (index == 0) ? true : false;


            //can only remove from the input and non Fly? or the first Slider
            if (isInputSide && !isTheFlyButton && !isTheOnlyInput)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

       
        IGH_Param IGH_VariableParameterComponent.CreateParameter(GH_ParameterSide side, int index)
        {
            Params.RegisterOutputParam(new Param_GenericObject(), 1);

            var param = new Param_GenericObject();
            param.Name = "Input";
            param.NickName = GH_ComponentParamServer.InventUniqueNickname("BCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            param.Description = "Please connect a Slider, Panel, or ValueList";
            
            //param.SetPersistentData(0.0);
            return param;
        }

        bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        void IGH_VariableParameterComponent.VariableParameterMaintenance()
        {


            for (int i = 0; i < this.Params.Input.Count; i++)
            {
                var testInput = new IteratorCheckAndGetInput();
                var validInput = testInput.CheckAndGetConnectedInputSource(this.Params.Input[i], this.Params.Output[i]);

            }


        }


        #endregion


        #region ParamInputChanged

        private void ParamInputChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("input changed!");
            //this.cr
            //IGH_Param newParam = null;
            //newParam.Name = "inputs";
            //newParam.NickName = "inputs";
            //newParam.Access = GH_ParamAccess.list;
            //this.Params.RegisterOutputParam(newParam,this.Params.Output.Count-1);
            ExpireSolution(true);
        }
        
        //private void ParamInputChanged(IGH_DocumentObject sender, GH_ParamServerEventArgs e)
        //{
        //    IGH_Param newParam = CreateParameter(GH_ParameterSide.Output, 1);
        //    ExpireSolution(true);
        //}

        #endregion


    }
}