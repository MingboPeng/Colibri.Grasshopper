using System;
using System.Collections.Generic;
using GH = Grasshopper;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Components;
using System.Threading.Tasks;

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
            pManager.AddGenericParameter("Input", "Input[N]", "Please connect a Slider, Panel, or ValueList", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Fly?", "Fly?", "Tell Colibri to fly!  Provide a button here, and click it once you are ready for Colibri to fly around your definition.", GH_ParamAccess.item,false);
            pManager[0].Optional = true;
            pManager[0].MutableNickName = false;
            pManager[1].MutableNickName = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "Value[N]", "current item of inputs", GH_ParamAccess.item);
            pManager.AddGenericParameter("Iteration ID", "FlyID", "connet to Aggregateor", GH_ParamAccess.list);
            pManager[0].MutableNickName = false;
            pManager[1].MutableNickName = false;
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



            //if (!_fly)
            //    return;

            //isReady = true;


            var validSources = new List<object>();
            var validindexList = new List<int>();

            for (int i = 0; i < this.Params.Input.Count; i++)
            {
                bool isFly = i == this.Params.IndexOfInputParam("Fly?") ? true : false;
                bool isEmptySource = this.Params.Input[i].SourceCount == 0 ? true : false;
                if (!isFly && !isEmptySource)
                {
                    validindexList.Add(i);
                    var validSource = IteratorParam.CheckAndGetValidInputSource(this.Params.Input[i]);
                    validSources.Add(validSource);
                    IteratorParam.ChangeParamNickName(validSource, this.Params.Input[i], this.Params.Output[i]);

                    //var paramValue = IteratorParam.GetParamAllStepIndex(validInputSource);
                    
                    //paramValue = paramValue.Contains(-1) ? paramValue[0] = "Unsupported conponent type! Please use Slider, Panel, or ValueList!" : paramValue;
                    //assign to output
                    //DA.SetDataList(i, paramValue);
                }

            }

            //FOR test purpose
            //var flyParam = new IteratorFlyParam(validSources);
            //flyParam.SetAllParamsStepIndexes();
            
            //for (int i = 0; i < flyParam.InputParamsStepIndexes.Count(); i++)
            //{
            //    DA.SetDataList(i, flyParam.InputParamsStepIndexes[i]);
            //}
            DA.SetDataList(this.Params.Output.Count - 1, validindexList);
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
                    string formatedUniqueNickName = "Input[N]";
                    param.NickName= formatedUniqueNickName;
                }
                param.Name = "Input";
                param.Description = "Please connect a Slider, Panel, or ValueList";
                param.Access = GH_ParamAccess.list;
                param.Optional = true;
                param.MutableNickName = false;

                var paramOutput = this.Params.Output[i];
                if (paramOutput.NickName == String.Empty)
                {

                    paramOutput.NickName = param.NickName.Replace("Input","Value");
                    paramOutput.Name = "Item";
                    paramOutput.Description = "This item is one of values from " + param.NickName;
                    paramOutput.MutableNickName = false;
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
            
           

            if (isInputSide && !isFly && !isSecondLastEmptySource)
            {
                //System.Windows.Forms.MessageBox.Show(e.ParameterIndex.ToString() + "_" + Params.IndexOfInputParam("Fly?").ToString());
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count - 1);
                Params.RegisterInputParam(newParam, Params.Input.Count - 1);
                VariableParameterMaintenance();
                this.Params.OnParametersChanged();
                this.ExpireSolution(true);
            }


        }

        #endregion


    }



}