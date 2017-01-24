using System;
using System.Collections.Generic;
using GH = Grasshopper;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Components;
using System.Threading.Tasks;
using Grasshopper.Kernel.Data;
using System.Windows.Forms;

namespace Colibri.Grasshopper
{
    public class Iterator2 : GH_Component, IGH_VariableParameterComponent
    {
        
        
        GH_Document doc = null;
        private bool Run = false;
        private bool Running = false;
        private bool IsAutoSetting = false;
        private List<ColibriParam> filteredSources;
        private IteratorFlyParam flyParam;

        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Iterator2()
          : base("Iterator2(fly)", "Iterator2",
              "Generates design iterations from a collection of sliders.",
              "TT Toolbox", "Colibri")
        {
            Params.ParameterSourcesChanged += ParamSourcesChanged;
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input", "Input[N]", "Please connect a Slider, Panel, or ValueList", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Steps", "Steps", ".....", GH_ParamAccess.item,false);
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
            
            //bool isReady = false;
            bool fly = false;
            DA.GetData(this.Params.Input.Count-1, ref fly);

            //var filteredSources = FilterSources();

            //Dictionary<string, string> FlyID = new Dictionary<string, string>();
            var FlyID = new List<object>();
            //Get current value

            if (filteredSources == null)
            {
                return;
            }

            bool isRunning = Run || Running;
            foreach (var item in filteredSources)
            {
                if (item != null)
                {
                    DA.SetData(item.AtIteratorPosition, item.CurrentValue(isRunning));
                    FlyID.Add(item.ToString(true));
                }
            }

            DA.SetDataList(Params.Output.Count()-1,FlyID);

            //fly
            if (Running)
            {
                return;
            }

            //if (IsAutoSetting)
            //{
            //    return;
            //}

            filteredSources.RemoveAll(item => item == null);
            foreach (var source in filteredSources)
            {
                source.Param.ObjectChanged -= Source_ObjectChanged;
                source.Param.ObjectChanged += Source_ObjectChanged;
            }


        }

        

        private void OnSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            // Unregister the event, we don't want to get called again.
            e.Document.SolutionEnd -= OnSolutionEnd;

            // If we're not supposed to run, abort now.
            if (!Run || Running)
                return;

            // Reset run and running states.
            Run = false;
            Running = true;


            try
            {
                flyParam.FlyAll(e);
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                // Always make sure that _running is switched off.
                Running = false;
                //e.Document.NewSolution(false);
                
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



        #region Collecting Source Params, and convert to Colibri Params

        /// <summary>
        /// convert current selected source to ColibriParam
        /// </summary>   
        private ColibriParam ConvertToColibriParam(IGH_Param SelectedSource, int AtIteratorPosition)
        {
            // Find the Guid for connected Slide or Panel

            var sources = SelectedSource; //list of things connected on this input
            
            var component = sources.Attributes.GetTopLevel.DocObject as IGH_Param; //for this connected thing, bring it into the code in a way where we can access its properties

            ColibriParam colibriParam = new ColibriParam(component, AtIteratorPosition);

            if (colibriParam.GHType != InputType.Unsupported)
            {
                //IsAutoSetting = true;
                var isNicknameEmpty = String.IsNullOrEmpty(colibriParam.NickName) || colibriParam.NickName == "List";
                colibriParam.NickName = isNicknameEmpty ? "RenamePlz" : colibriParam.NickName; //Check if nick name exists
                colibriParam.Param.ExpireSolution(false);
                //IsAutoSetting = false;

                //Flatten the Panel's data just in case 
                if (colibriParam.GHType == InputType.Panel)
                {
                    sources.VolatileData.Flatten();
                }
            }
            else
            {
                colibriParam = null;
                MessageBox.Show("Please check all inputs! \nSlider, ValueList, or Panel are only supported!");
            }



            return colibriParam;


        }

        /// <summary>
        /// Change Iterator's NickName
        /// </summary>   
        public void ChangeParamNickName(ColibriParam ValidSourceParam)
        {

            if (ValidSourceParam != null)
            {
                
                var validSourceParam = ValidSourceParam;
                int atPosition = validSourceParam.AtIteratorPosition;

                var inputParam = this.Params.Input[atPosition];
                var outputParam = this.Params.Output[atPosition];


                var type = validSourceParam.GHType;
                bool isTypeUnsupported = type == InputType.Unsupported ? true : false;

                inputParam.NickName = isTypeUnsupported ? type.ToString() : validSourceParam.NickName;
                outputParam.NickName = inputParam.NickName;
                outputParam.Description = "This item is one of values from " + type.ToString() + "_" + inputParam.NickName;
                
            }


        }

        /// <summary>
        /// check input source if is slider, panel, or valueList, if not, remove it
        /// </summary>   
        private List<ColibriParam> FilterSources()
        {
            var filtedSources = new List<ColibriParam>();
            //var validindexList = new List<int>();
            for (int i = 0; i < this.Params.Input.Count; i++)
            {
                //Check if it is fly or empty source param
                bool isFly = i == this.Params.Input.Count - 1 ? true : false;
                var source = this.Params.Input[i].Sources;
                bool ifAny = source.Any();

                if (!isFly && ifAny)
                {
                    //if something's connected,and get the first connected
                    var filtedSource = ConvertToColibriParam(source[0], i);
                    //null  added if input is unsupported type
                    if (filtedSource != null)
                    {
                        filtedSources.Add(filtedSource);
                        ChangeParamNickName(filtedSource);
                    }
                    
                }
            }

            // remove all unvalid items 
            //filteredSources.RemoveAll(item => item == null);

            return filtedSources;
        }
        #endregion
        

        #region Button on Iterator
        // Create Button
        public override void CreateAttributes()
        {
            var newButtonAttribute = new ColibriParameterAttributes(this);
            newButtonAttribute.btnText = "Fly";
            newButtonAttribute.mouseDownEvent += OnMouseDownEvent;
            m_attributes = newButtonAttribute;

        }
        // response to Button event
        private void OnMouseDownEvent(object sender)
        {
            if (doc == null)
            {
                doc = GH.Instances.ActiveCanvas.Document;
            }
            //Clean first
            this.flyParam = null;

            //recollect all params 
            //var filteredSources = FilterSources();

            filteredSources.RemoveAll(item => item == null);
            filteredSources.RemoveAll(item => item.GHType == InputType.Unsupported);

            if (filteredSources.Count() > 0)
            {
                this.flyParam = new IteratorFlyParam(filteredSources);
            }
            else
            {
                MessageBox.Show("No Valid Slider, ValueList, or Panel connected!");
                return;
            }



            var userClick = MessageBox.Show(flyParam.InputParams.Count() + " slider(s) connected:\n" + "Param Names " +
                  "\n" + flyParam.TotalIterations + " iterations will be done. Continue?" + "\n\n (Press ESC to pause during progressing!)", "Start?", MessageBoxButtons.YesNo);

            if (userClick == DialogResult.Yes)
            {
                Run = true;
                //flyParam.FirstResetAll();
                doc.SolutionEnd += OnSolutionEnd;

                // only recompute those are expired flagged
                doc.NewSolution(false);

            }
        }

        #endregion


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
            bool isTheFlyButton = (index == this.Params.Input.Count-1) ? true : false;
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
            output.Access = GH_ParamAccess.item;
            Params.RegisterOutputParam(output, index);

            var param = new Param_GenericObject();
            param.NickName = String.Empty;

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


        #region Events
        //This is for if any source connected, removed, 
        private void ParamSourcesChanged(Object sender, GH_ParamServerEventArgs e)
        {

            //WIP
            
            bool isInputSide = e.ParameterSide == GH_ParameterSide.Input ? true : false;
            bool isFly = e.ParameterIndex == this.Params.Input.Count-1 ? true : false;
            bool isSecondLastEmptySource = Params.Input[this.Params.Input.Count - 2].SourceCount == 0 ? true : false;

            

            if (isInputSide && !isFly && !isSecondLastEmptySource)
            {
                // add a new input param
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count - 1);
                Params.RegisterInputParam(newParam, Params.Input.Count - 1);
                VariableParameterMaintenance();
                this.Params.OnParametersChanged();
                
                this.ExpireSolution(true);

            }
            
            //recollecting the filteredSources and rename
            filteredSources = FilterSources();

        }
        
        
        //This is for if any source name changed
        private void Source_ObjectChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            //this.flyParam = null;
            this.filteredSources = FilterSources();
            this.ExpireSolution(true);
        }

        
        #endregion


    }



}