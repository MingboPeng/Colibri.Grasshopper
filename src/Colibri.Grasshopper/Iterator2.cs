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
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

namespace Colibri.Grasshopper
{
    public class Iterator2 : GH_Component, IGH_VariableParameterComponent
    {
        
        private GH_Document _doc = null;
        private bool _run = false;
        private bool _running = false;
        private List<ColibriParam> _filteredSources;
        private IteratorFlyParam _flyParam;
        private bool _isTestFly = false;
        private bool _ignoreAllWarningMsg = false;
        private string _studyFolder = "";

        private int _totalCount = 0;
        private int _selectedCount = 0;

        private IteratorSelection _selections = new IteratorSelection();


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Iterator2()
          : base("Colibri FlyID (Iterator)", "Iterator",
              "Generates design iterations from a collection of sliders, panels, or valueLists.",
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
            pManager[0].Optional = true;
            pManager[0].MutableNickName = false;

            pManager.AddGenericParameter("Selection", "Selection", "Optional input if you want to run partial iterations.", GH_ParamAccess.item);
            pManager[1].Optional = true;
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
            var userSelections = new IteratorSelection();
            DA.GetData(this.Params.Input.Count - 1, ref userSelections);
            
            //Dictionary<string, string> FlyID = new Dictionary<string, string>();
            var FlyID = new List<object>();
            
            //flyParam only exists when flying
            if (!_running && _flyParam == null)
            {
                
                _filteredSources = gatherSources();
                checkAllInputParamNames(_filteredSources);

                this._selections = new IteratorSelection(userSelections.UserTakes, userSelections.UserDomains);
                this.Message = updateComponentMsg(_filteredSources, this._selections);
                
            }
            
            //Get current value
            foreach (var item in _filteredSources)
            {
                DA.SetData(item.AtIteratorPosition, item.CurrentValue());
                FlyID.Add(item.ToString(true));
            }
            
            DA.SetDataList(Params.Output.Count-1,FlyID);
            
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
                return Properties.Resources.Iterator;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{74a79561-b3b2-4e12-beb4-d79ec0ed378a}"); }
        }

        #region Override
        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            foreach (var item in _filteredSources)
            {
                item.ObjectNicknameChanged -= OnSourceNicknameChanged;
            }
        }

        public override bool Read(GH_IReader reader)
        {

            if (reader.ItemExists("ignoreAllWarningMsg"))
            {
                _ignoreAllWarningMsg = reader.GetBoolean("ignoreAllWarningMsg");
            }

            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ignoreAllWarningMsg", _ignoreAllWarningMsg);
            return base.Write(writer);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Fly Test", runFlyTest);
            //Menu_AppendSeparator(menu);

            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Ignore all warning messages", ignoreWarningMsg, true, _ignoreAllWarningMsg);
            Menu_AppendSeparator(menu);
        }

        // Create Button
        public override void CreateAttributes()
        {
            var newButtonAttribute = new IteratorAttributes(this) { ButtonText = "Fly" };
            newButtonAttribute.mouseDownEvent += OnMouseDownEvent;
            m_attributes = newButtonAttribute;

        }
        #endregion


        
        #region Collecting Source Params, and convert to Colibri Params

        /// <summary>
        /// convert current selected source to ColibriParam
        /// </summary>   
        private ColibriParam ConvertToColibriParam(IGH_Param SelectedSource, int AtIteratorPosition)
        {
            
            //var component = SelectedSource; //list of things connected on this input
            
            ColibriParam colibriParam = new ColibriParam(SelectedSource, AtIteratorPosition);

            //if (colibriParam.GHType == InputType.Unsupported)
            //{
            //    return null;
            //}
            //else
            //{
                //Flatten the Panel's data just in case 
                if (colibriParam.GHType == InputType.Panel)
                {
                    SelectedSource.VolatileData.Flatten();
                }
                
                return colibriParam;
            //}
            
        }

        ///// <summary>
        ///// Change Iterator's NickName
        ///// </summary>   
        //private void checkSourceParamNickname(ColibriParam colibriParam)
        //{
            
        //    //Check if nickname exists
        //    var isNicknameEmpty = String.IsNullOrEmpty(colibriParam.NickName) || colibriParam.NickName == "List";
        //    if (isNicknameEmpty)
        //    {
        //        colibriParam.NickName = "RenamePlz";
        //    }
            
        //}

        /// <summary>
        /// Change Iterator's input and output NickName
        /// </summary>   
        private void checkInputParamNickname(ColibriParam ValidSourceParam)
        {

            if (ValidSourceParam != null)
            {
                var colibriParam = ValidSourceParam;
                
                int atPosition = colibriParam.AtIteratorPosition;

                var inputParam = this.Params.Input[atPosition];
                var outputParam = this.Params.Output[atPosition];

                string newNickname = colibriParam.NickName;
                inputParam.NickName = newNickname;
                outputParam.NickName = newNickname;
                outputParam.Description = "This item is one of values from " + colibriParam.GHType.ToString() + "_" + newNickname;
                
                this.Attributes.ExpireLayout();
            }
            
        }

        /// <summary>
        /// check input source if is slider, panel, or valueList, if not, remove it
        /// </summary>   
        private List<ColibriParam> gatherSources()
        {
            var filtedSources = new List<ColibriParam>();
            
            // exclude the last input which is "Selection"
            for (int i = 0; i < this.Params.Input.Count-1; i++)
            {
                //Check if it is fly or empty source param
                //bool isFly = i == this.Params.Input.Count - 1 ? true : false;
                var source = this.Params.Input[i].Sources;
                //bool ifAny = source.Any();

                if (source.Any() && source.Count==1)
                {
                    //if something's connected,and get the last connected
                    var colibriParam = ConvertToColibriParam(source.Last(), i);

                    //null  added if input is unsupported type
                    if (colibriParam.GHType != InputType.Unsupported)
                    {

                        colibriParam.ObjectNicknameChanged += OnSourceNicknameChanged;
                        //colibriParam.RawParam.ObjectChanged += OnSource_ObjectChanged;OnSource_ObjectNicknameChanged
                        filtedSources.Add(colibriParam);
                        
                    }
                    else
                    {
                        //throw new ArgumentException("Unsupported component!\nPlease use Slider, ValueList, or Panel!");
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Unsupported component!\nPlease use Slider, ValueList, or Panel!");
                        break;
                        //return null;
                    }
                    
                }
                else if(source.Count > 1)
                {
                    //throw new ArgumentException("Please connect one component per grip!");
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Please connect one component per grip!");
                    break;
                    //return null;
                }
            }
            
            return filtedSources;
        }

        /// <summary>
        /// check and iterator's input and output params' nicknames
        /// </summary>   
        private void checkAllInputParamNames(List<ColibriParam> validColibriParams)
        {
            //all items in the list are unnull. which is checked in gatherSources();
            
            if (validColibriParams.IsNullOrEmpty()) return;
            
            foreach (var item in validColibriParams)
            {
                //checkSourceParamNickname(item);
                checkInputParamNickname(item);
            }
        }

        ///// <summary>
        ///// check input source Nicknames
        ///// </summary>   
        //private void checkAllSourceParamNames(List<ColibriParam> validColibriParams)
        //{
        //    //all items in the list are unnull. which is checked in gatherSources();

        //    if (validColibriParams.IsNullOrEmpty()) return;

        //    foreach (var item in validColibriParams)
        //    {
        //        checkSourceParamNickname(item);
        //        //checkInputParamNickname(item);
        //    }
        //}


        #endregion


        #region Button Event on Iterator

        // response to Button event
        private void OnMouseDownEvent(object sender)
        {
            if (_doc == null)
            {
                _doc = GH.Instances.ActiveCanvas.Document;
            }

            if (this.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error) return;
           
            //Clean first
            this._flyParam = null;
            
            //recollect all params 
            _filteredSources = gatherSources();

            _filteredSources.RemoveAll(item => item == null);
            _filteredSources.RemoveAll(item => item.GHType == InputType.Unsupported);

            
            //checked if Aggregator is recording and the last
            if (!isAggregatorReady()) return;
           
            //check if any vaild input source connected to Iteratior
            if (_filteredSources.Count() > 0)
            {
                this._flyParam = new IteratorFlyParam(_filteredSources,this._selections,this._studyFolder);
            }
            else
            {
                MessageBox.Show("No Slider, ValueList, or Panel connected!");
                return;
            }

            
            int testIterationNumber = _selectedCount;
            //int totalIterationNumber = _totalCount;

            if (_isTestFly)
            {
                testIterationNumber = 3;
            }

            var userClick = MessageBox.Show(_flyParam.InputParams.Count() + " slider(s) connected:\n" + "Param Names " +
                  "\n\n" + testIterationNumber + " (out of "+ _totalCount + ") iterations will be done. Continue?" + "\n\n (Press ESC to pause during progressing!)", "Start?", MessageBoxButtons.YesNo);

            if (userClick == DialogResult.Yes)
            {
                _run = true;
                _doc.SolutionEnd += OnSolutionEnd;
                
                // only recompute those are expired flagged
                _doc.NewSolution(false);
            }
            else
            {
                _run = false;
                _isTestFly = false;
            }


        }
        
        private void runFlyTest(object sender, EventArgs e)
        {
            _isTestFly = true;
            this.OnMouseDownEvent(sender);
        }

        private void OnSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            // Unregister the event, we don't want to get called again.
            e.Document.SolutionEnd -= OnSolutionEnd;

            // If we're not supposed to run, abort now.
            if (!_run || _running)
                return;

            // Reset run and running states.
            _run = false;
            _running = true;

            try
            {
                if (_isTestFly)
                {
                    _isTestFly = false;
                    _flyParam.FlyTest(e, 3);
                }
                else
                {
                    _flyParam.FlyAll(e);
                }

                if (_aggObj != null)
                {
                    _aggObj.setWriteFileToFalse();
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                // Always make sure that _running is switched off.
                _running = false;
                this._flyParam = null;

            }

        }

        private void ignoreWarningMsg(object sender, EventArgs e)
        {
            _ignoreAllWarningMsg = !_ignoreAllWarningMsg;
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
            bool isTheOnlyInput = (index == 0)&&(this.Params.Input.Count<=2) ? true : false;


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
            var outParam = new Param_GenericObject();
            outParam.NickName = String.Empty;
            Params.RegisterOutputParam(outParam, index);

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


        //Todo remove unnecessary code here
        public void VariableParameterMaintenance()
        {
           
            for (int i = 0; i < this.Params.Input.Count-1; i++)
            {
                // create inputs
                var inParam = this.Params.Input[i];
                if (inParam.NickName == String.Empty) {
                    inParam.NickName= "Input[N]";
                }
                inParam.Name = "Input";
                inParam.Description = "Please connect a Slider, Panel, or ValueList";
                inParam.Access = GH_ParamAccess.list;
                inParam.Optional = true;
                inParam.MutableNickName = false;
                inParam.WireDisplay = GH_ParamWireDisplay.faint;

                var outParam = this.Params.Output[i];
                if (outParam.NickName == String.Empty)
                {
                    outParam.NickName = inParam.NickName.Replace("Input","Value");
                    outParam.Description = "This item is one of values from " + inParam.NickName;
                    outParam.Access = GH_ParamAccess.item;
                    outParam.Name = "Item";
                    outParam.MutableNickName = false;
                }
            }
            
        }


        #endregion


        #region Events for ParamSourcesChanged
        
        //This is for if any source connected, reconnected, removed, replacement 
        private void ParamSourcesChanged(Object sender, GH_ParamServerEventArgs e)
        {
            
            bool isInputSide = e.ParameterSide == GH_ParameterSide.Input ? true : false;
            bool isSelection = e.ParameterIndex == this.Params.Input.Count-1 ? true : false;

            
            //check input side only
            if (!isInputSide) return;

            //check if is Selection setting only
            if (isSelection) return;


            bool isSecondLastSourceFull = Params.Input[this.Params.Input.Count - 2].Sources.Any();
            // add a new input param while the second last input is full
            if (isSecondLastSourceFull)
            {
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count - 1);
                Params.RegisterInputParam(newParam, Params.Input.Count - 1);
                VariableParameterMaintenance();
                this.Params.OnParametersChanged();
            }
            
            ////recollecting the filteredSources and rename while any source changed
            //_filteredSources = gatherSources();
            //checkAllNames(filteredSources);

        }
        
        //This is for if any source name changed, NOTE: cannot deteck the edited
        private void OnSourceNicknameChanged(ColibriParam sender)
        {
            //bool isExist = _filteredSources.Exists(_ => _.RawParam.Equals(sender));
            bool isExist = _filteredSources.Contains(sender);

            if (isExist)
            {
                
                //if (e.Type == GH_ObjectEventType.NickName)
                //{
                    checkAllInputParamNames(_filteredSources);
                    //this.ExpireSolution(true);

                    //edit the Fly output without expire this component's solution, 
                    // only expire the downstream components which connected to the last output "FlyID"
                    this.Params.Output.Last().ExpireSolution(false);
                    this.Params.Output.Last().ClearData();
                    this.Params.Output.Last().AddVolatileDataList(new GH_Path(0, 0), getFlyID());
                    
                    //foreach (var item in this.Params.Output.Last().Recipients)
                    //{
                    //    item.ExpireSolution(false);
                    //}

                    if (_doc == null) _doc = GH.Instances.ActiveCanvas.Document;

                    _doc.NewSolution(false);

                //}

            }

            else
            {
                sender = null;
            }
            
        }

        private List<string> getFlyID()
        {
            if (_filteredSources.IsNullOrEmpty()) return new List<string>();

            var FlyID = new List<string>();
            foreach (var item in _filteredSources)
            {
                FlyID.Add(item.ToString(true));
            }

            return FlyID;
        }
       
        private string updateComponentMsg(List<ColibriParam> ColibriParams, IteratorSelection Selections)
        {
            
            if (ColibriParams.IsNullOrEmpty())
            {
                return null;
            }
            
            //this will check and add take_numbers and domains
            Selections.MatchSelectionFrom(ColibriParams);
            _totalCount = Selections.TotalCounts;
            _selectedCount = Selections.SelectedCounts;
            string messages = "";

            //Check selections
            checkSelections(Selections, ColibriParams, _totalCount);

            messages = "ITERATION NUMBER \nTotal: " + _totalCount;

            if (Selections.IsDefinedInSel)
            {
                messages += "\nSelected: " + _selectedCount;
                messages += "\n\n-----SELECTION-----\n";
                messages += Selections.ToString(true);
            }
            
            
            return messages;
            
        }

        private void checkSelections(IteratorSelection Selections, List<ColibriParam> ColibriParam, int totalCount)
        {
            var takeNumbers = new List<int>();
            var userDomains = new List<GH_Interval>();
            

            if (Selections.IsDefinedInSel)
            {
                takeNumbers = Selections.UserTakes;
                userDomains = Selections.UserDomains;
                //check take numbers for each parameters
                if (takeNumbers.Any() && takeNumbers.Count != ColibriParam.Count)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The number of connected sliders must be equal to the number of items in the Steps input list.\n But Colibri will run all iterations in this case.");
                    
                }


                //Check domains if any of their max is out of range (the min is checked in Selection component)
                if (userDomains.Any())
                {
                    foreach (var item in userDomains)
                    {

                        if (item.Value.Max > totalCount-1)
                        {
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Domains' max number should be smaller than the total number " + totalCount + ".\n Colibri has fixed it for you.");
                        }
                        
                    }
                    
                }

            }
            

        }

        #endregion

        #region Check Aggregator before fly
        private Aggregator _aggObj = null;
        
        //Check if Aggregator exist, and if it is at the last
        private bool isAggregatorReady()
        {
            
            var folder = "";
            bool isReady = true;
            _aggObj = aggregatorObj();
            var checkingMsg = new List<string>();

            //is aggregator is not connected 
            if (_aggObj == null) return isReady;
            
            //check aggregator 
            checkingMsg = _aggObj.CheckAggregatorIfReady();

            if (checkingMsg.IsNullOrEmpty() || _ignoreAllWarningMsg)
            {
                isReady = true;
            }
            else
            {
                string warningMsg = "";
                foreach (var item in checkingMsg)
                {
                    warningMsg += "\n\n"+item;
                }
                var userClick = MessageBox.Show("Colibri detected some issues. \nStill continue?\n" + warningMsg, "Attention", MessageBoxButtons.YesNo);
                if (userClick == DialogResult.No)
                {
                    // user doesn't want ot continue! set isReady to false to stop
                    isReady = false;
                }
            }
                
            folder = _aggObj.Folder;
            

            _studyFolder = folder;
            return isReady;

        }
        
        private Aggregator aggregatorObj()
        {
            _aggObj = null;
            var aggregatorID = new Guid("{787196c8-5cc8-46f5-b253-4e63d8d271e1}");

            // only check Recipients of FlyID
            var flyIDRecipients = this.Params.Output.Last().Recipients;

            if (flyIDRecipients.IsNullOrEmpty()) return null;

            //var aggObj = flyIDRecipients.Where(
            //    _ => _.Attributes.GetTopLevel.DocObject.ComponentGuid.Equals(aggregatorID)
            //    ).FirstOrDefault();

            //_aggObj = aggObj.Attributes.GetTopLevel.DocObject as Aggregator;

            //_aggObj = (from item in flyIDRecipients
            //           where item.Attributes.GetTopLevel.DocObject.ComponentGuid.Equals(aggregatorID)
            //           select item.Attributes.GetTopLevel.DocObject)
            //           .First() as Aggregator;

            foreach (var item in flyIDRecipients)
            {
                var recipientParent = item.Attributes.GetTopLevel.DocObject;
                if (recipientParent.ComponentGuid.Equals(aggregatorID))
                {
                    _aggObj = recipientParent as Aggregator;
                }
            }
            return _aggObj;
        }


        #endregion

        

    }



}