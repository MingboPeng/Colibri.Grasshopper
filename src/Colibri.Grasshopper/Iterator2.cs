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
          : base("Iterator(Fly)", "Iterator",
              "Description",
              "Colibri", "Colibri")
        {
            Params.ParameterSourcesChanged += ParamInputChanged;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input", "A", "Please connect a Slider, Panel, or ValueList", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Fly?", "Fly?", "Tell Colibri to fly!  Provide a button here, and click it once you are ready for Colibri to fly around your definition.", GH_ParamAccess.item,false);
            
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

            bool isReady = false;
            bool _fly = false;
            DA.GetData(this.Params.IndexOfInputParam("Fly?"), ref _fly);


            //var validInputSource = IteratorParam.CheckAndGetConnectedInputSource(this.Params.Input[0]);
            //IteratorParam.changeParamNickName(validInputSource, this.Params.Input[0], this.Params.Output[0]);

            //if (validInputSource.Any())
            //{
            //    //    IGH_Param newParam = null;
            //    //    newParam.Name = "inputs";
            //    //    newParam.NickName = "inputs";
            //    //    newParam.Access = GH_ParamAccess.list;
            //    //    this.Params.RegisterOutputParam(newParam, this.Params.Output.Count - 1);


            //    //    //http://www.grasshopper3d.com/forum/topics/dynamic-outputs-for-component

            //    //    //    //not working
            //    //    //    // GH_Component.GH_InputParamManager newParamManager = null;
            //    //    //    // newParamManager.AddGenericParameter("inputs", "inputs", "Slider or Panel", GH_ParamAccess.list);
            //    //    //    // this.RegisterInputParams(newParamManager);


            //}

            //if (!_fly)
            //    return;

            //isReady = true;
            
            
            var validInputSource = new Dictionary<IteratorParam.InputType, IGH_Param>();
            for (int i = 0; i < this.Params.Input.Count; i++)
            {
                bool isFly = i == this.Params.IndexOfInputParam("Fly?") ? true : false;
                bool isEmptySource = this.Params.Input[i].SourceCount==0 ? true : false;
                if (!isFly && !isEmptySource)
                {
                    validInputSource = IteratorParam.CheckAndGetConnectedInputSource(this.Params.Input[i]);
                    IteratorParam.changeParamNickName(validInputSource, this.Params.Input[i], this.Params.Output[i]);
                }   
                    

            }

            

            //CreateParameter(GH_ParameterSide.Input, 1);
            //assign to output
            DA.SetDataList(0, validInputSource);

           
            
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
        public bool CanInsertParameter(GH_ParameterSide side, int index)
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

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
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


        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            var output = new Param_GenericObject();
            output.NickName = String.Empty;
            Params.RegisterOutputParam(output, index);

            var param = new Param_GenericObject();
            param.NickName = String.Empty;

            //param.Name = "Input";
            //param.NickName = GH_ComponentParamServer.InventUniqueNickname("BCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
            //param.Description = "Please connect a Slider, Panel, or ValueList";
            
            //param.SetPersistentData(0.0);
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            //unregister ther output when input is destroied.
            Params.UnregisterOutputParameter(Params.Output[index]);
            return true;
        }

        public void VariableParameterMaintenance()
        {
           
            for (int i = 0; i < this.Params.Input.Count-1; i++)
            {
                // create inputs
                var param = this.Params.Input[i];
                if (param.NickName == String.Empty) {
                    param.NickName= GH_ComponentParamServer.InventUniqueNickname("BCDEFGHIJKLMNOPQRSTUVWXYZ", Params.Input);
                }
                param.Name = "Input";
                param.Description = "Please connect a Slider, Panel, or ValueList";
                param.Access = GH_ParamAccess.list;
                param.Optional = true;

                var paramOutput = this.Params.Output[i];
                if (paramOutput.NickName == String.Empty)
                {
                    paramOutput.NickName = param.NickName;
                    paramOutput.Name = "Item";
                    paramOutput.Description = "This item is one of values from " + param.NickName;
                }
            }

          
        }


        #endregion


        #region ParamInputChanged

        private void ParamInputChanged(Object sender, GH_ParamServerEventArgs e)
        {

            //WIP

            bool isInputSide = e.ParameterSide == GH_ParameterSide.Input ? true : false;
            bool isFly = e.ParameterIndex == this.Params.IndexOfInputParam("Fly?") ? true : false;
            bool isSecondLastEmptySource = Params.Input[this.Params.Input.Count - 2].SourceCount == 0 ? true : false;
            
            //System.Windows.Forms.MessageBox.Show(isInputSide + "_" + isFly + "_" + isSecondLastEmptySource);

            if (isInputSide && !isFly && !isSecondLastEmptySource)
            {
                //System.Windows.Forms.MessageBox.Show(e.ParameterIndex.ToString() + "_" + Params.IndexOfInputParam("Fly?").ToString());
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count - 1);
                Params.RegisterInputParam(newParam, Params.Input.Count - 1);
                VariableParameterMaintenance();
                Params.OnParametersChanged();
                ExpireSolution(true);

            }


        }

        #endregion


    }



}