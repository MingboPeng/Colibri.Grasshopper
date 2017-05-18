using System;
using System.Collections.Generic;
using GH = Grasshopper;
using Grasshopper.Kernel;
using System.Linq;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Data;
using System.Windows.Forms;
using Grasshopper.Kernel.Types;
using GH_IO.Serialization;

namespace Colibri.Grasshopper
{
    public class Iterator : GH_Component, IGH_VariableParameterComponent
    {
        
        private GH_Document _doc = null;
        private bool _run = false;
        private bool _running = false;
        private List<ColibriParam> _filteredSources;
        private IteratorFlyParam _flyParam;
        private bool _isTestFly = false;
        
        
        private int _totalCount = 0;
        private int _selectedCount = 0;

        private IteratorSelection _selections = new IteratorSelection();

        private Aggregator _aggObj = null;
        private OverrideMode _mode = OverrideMode.AskEverytime;
        private bool _ignoreAllWarningMsg = false;
        private bool _remoteFly = false;
        private string _selectionName = "Selection";
        private string _remoteFlyName = "RemoteFly";
        private string _remoteCtrlName = "RemoteCtrl";
        private string _studyFolder = "";


        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Iterator()
          : base("Colibri Iterator", "Iterator",
              "Generates design iterations from a collection of sliders, panels, or valueLists.",
              "TT Toolbox", "Colibri 2.0")
        {
            Params.ParameterSourcesChanged += ParamSourcesChanged;
            
        }
        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Input", "Input[N]", "Please connect a Slider, Panel, or ValueList, as a variable input", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[0].MutableNickName = false;

            pManager.AddGenericParameter(this._selectionName, this._selectionName, "Optional input if you want to run partial iterations.", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager[1].MutableNickName = false;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "Value[N]", "current item of inputs", GH_ParamAccess.item);
            pManager.AddGenericParameter("Iteration Genome", "Genome", "Contains a collection of genes (variables) which defines a unique ID of each iteration. Connet to Aggregateor", GH_ParamAccess.list);
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
            bool remoteFly = false;
            int selectionIndex = this.Params.IndexOfInputParam(this._selectionName);
            DA.GetData(selectionIndex, ref userSelections);

            if (this._remoteFly)
            {
                DA.GetData(selectionIndex + 1, ref remoteFly);

                //set remoteCtrl 
                DA.SetData(selectionIndex + 1, _running);
                
            }

            
            
            var FlyID = new List<object>();
            
            //flyParam only exists when flying
            if (!_running && _flyParam == null)
            {
                _filteredSources = gatherSources();
                checkAllInputParamNames(_filteredSources);

                this._selections = new IteratorSelection(userSelections.UserTakes, userSelections.UserDomains);
                this.Message = updateComponentMsg(_filteredSources, this._selections);

                if (remoteFly)
                {
                    this.OnMouseDownEvent(this);
                }
            }
            
            //Get current value
            foreach (var item in _filteredSources)
            {
                DA.SetData(item.AtIteratorPosition, item.CurrentValue());
                FlyID.Add(item.ToString(true));
            }
            
            DA.SetDataList(selectionIndex, FlyID);
            
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
            if (_filteredSources.IsNullOrEmpty()) return;
            foreach (var item in _filteredSources)
            {
                item.ObjectNicknameChanged -= OnSourceNicknameChanged;
            }
        }

        public override bool Read(GH_IReader reader)
        {

            if (reader.ItemExists("ignoreAllWarningMsg"))
            {
                this._ignoreAllWarningMsg = reader.GetBoolean("ignoreAllWarningMsg");
            }
            if (reader.ItemExists(this._remoteFlyName))
            {
                this._remoteFly = reader.GetBoolean(this._remoteFlyName);
            }

            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("ignoreAllWarningMsg", this._ignoreAllWarningMsg);
            writer.SetBoolean(this._remoteFlyName, this._remoteFly);
            return base.Write(writer);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Fly Test", runFlyTest);
            //Menu_AppendSeparator(menu);

            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "Ignore all warning messages", ignoreWarningMsg, true, this._ignoreAllWarningMsg);

            base.AppendAdditionalComponentMenuItems(menu);
            Menu_AppendItem(menu, "RemoteFly", remoteFly, true, this._remoteFly);
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
            
            //Flatten the Panel's data just in case 
            if (colibriParam.GHType == InputType.Panel)
            {
                SelectedSource.VolatileData.Flatten();
            }
            return colibriParam;
        }
        
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

            int selectionIndex = this.Params.IndexOfInputParam(this._selectionName);
            // exclude the last input which is "Selection"
            for (int i = 0; i < selectionIndex; i++)
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


        #endregion

        #region Button Event on Iterator

        // response to Button event
        private void OnMouseDownEvent(object sender)
        {
            if (this._doc == null)
            {
                this._doc = GH.Instances.ActiveCanvas.Document;
            }

            if (this.RuntimeMessageLevel == GH_RuntimeMessageLevel.Error) return;
           
            //Clean first
            this._flyParam = null;

            //recollect all params 
            this._filteredSources = gatherSources();

            this._filteredSources.RemoveAll(item => item == null);
            this._filteredSources.RemoveAll(item => item.GHType == InputType.Unsupported);
            //string Para
            
            //checked if Aggregator is recording and the last
            if (!isAggregatorReady()) return;
           
            //check if any vaild input source connected to Iteratior
            if (this._filteredSources.Count() > 0)
            {
                this._flyParam = new IteratorFlyParam(_filteredSources,this._selections,this._studyFolder, this._mode);
            }
            else
            {
                MessageBox.Show("No Slider, ValueList, or Panel connected!");
                return;
            }

            
            int testIterationNumber = this._selectedCount;
            //int totalIterationNumber = _totalCount;

            if (this._isTestFly)
            {
                testIterationNumber = 3;
            }
            string msgString = _flyParam.InputParams.Count() + " input(s) connected." +
                  "\n" + testIterationNumber + " (out of " + _totalCount + ") iterations will be done. \n\nContinue?" +
                  "\n\n-------------------------------------------------------- "+
                  "\nTo pause during progressing?\n    1.Press ESC.";

            if (!String.IsNullOrWhiteSpace(this._studyFolder))
            {
                msgString += "\n    2.Or remove \"running\" file located in foler:\n\t" + this._studyFolder;
            }

            var userClick = MessageBox.Show(msgString, "Start?", MessageBoxButtons.YesNo);

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
                _flyParam = null;
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
                    _flyParam.Fly(e);
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
            this._ignoreAllWarningMsg = !this._ignoreAllWarningMsg;
        }
        

        private void remoteFly(object sender, EventArgs e)
        {
            this._remoteFly = !this._remoteFly;
            
            if (this._remoteFly)
            {
                var remoteInParam = new Param_Boolean();
                var remoteOutParam = new Param_Boolean();

                int index = this.Params.Input.Count;
                this.Params.RegisterInputParam(remoteInParam, index);
                this.Params.RegisterOutputParam(remoteOutParam, index);

            }
            else
            {
                int remoteFlyIndex = this.Params.IndexOfInputParam(this._remoteFlyName);
                this.Params.UnregisterInputParameter(this.Params.Input[remoteFlyIndex]);
                this.Params.UnregisterOutputParameter(this.Params.Output[remoteFlyIndex]);
            }
            VariableParameterMaintenance();
            this.Params.OnParametersChanged();
            this.ExpireSolution(true);

        }

        #endregion

        #region Methods of IGH_VariableParameterComponent interface

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            bool isInputSide = side == GH_ParameterSide.Input;
            
            bool isTheOnlyInput = index == 0;

            //isSetting includes Selection and remoteFly,
            //canInsert at the end when remoteFly is flase.
            bool isSetting = false;
            if (this._remoteFly)
            {
                //if remoteFly is on, then the last two (Selection and remoteFly) can not insert anymore.
                isSetting = (index == this.Params.Input.Count) || (index == this.Params.Input.Count-1);
            }
            
            //We only let input parameters to be added 
            if (isInputSide && !isSetting && !isTheOnlyInput)
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
            bool isInputSide = side == GH_ParameterSide.Input;
            //bool isTheFlyButton = index == this.Params.Input.Count-1;
            bool isTheOnlyInput = (index == 0)&&(this.Params.Input.Count<=2);


            //isSetting includes Selection and remoteFly,
            //cannot remove Selection Setting.
            bool isSetting = index == this.Params.Input.Count - 1;
            if (this._remoteFly)
            {
                //if remoteFly is on, then Selection is at this.Params.Input.Count-2.
                isSetting = (index == this.Params.Input.Count-2);
            }

            //can only remove from the input and non Fly? or the first Slider
            if (isInputSide && !isSetting && !isTheOnlyInput)
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
            
            // if remoteFly
            if (index == this.Params.Input.Count)
            {
                this._remoteFly = true;
            }

            if (this._remoteFly && (index == this.Params.Input.Count))
            {
                var remoteInParam = new Param_Boolean();
                var remoteOutParam = new Param_Boolean();
                Params.RegisterOutputParam(remoteOutParam, index);
                return remoteInParam;
            }

            // add normal params
            var outParam = new Param_GenericObject();
            outParam.NickName = String.Empty;
            Params.RegisterOutputParam(outParam, index);

            var inParam = new Param_GenericObject();
            inParam.NickName = String.Empty;
            return inParam;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            //bool isRemoteFly = index == this.Params.IndexOfInputParam(this._remoteFlyName)
            if (this._remoteFly && (index == this.Params.Input.Count-1))
            {
                this._remoteFly = false;
            }

            //unregister ther output when input is destroied.
            Params.UnregisterOutputParameter(Params.Output[index]);

            
            return true;
        }


        //Todo remove unnecessary code here
        public void VariableParameterMaintenance()
        {
            int inputParamCount = this.Params.Input.Count - 1;
            if (this._remoteFly)
            {
                inputParamCount--; //this.Params.Input.Count - 2;

                //settings for remoteFly
                var remoteInParam = this.Params.Input.Last() as Param_Boolean;
                remoteInParam.Name = this._remoteFlyName;
                remoteInParam.NickName = this._remoteFlyName;
                remoteInParam.Access = GH_ParamAccess.item;
                //remoteInParam.SetPersistentData(new GH_Boolean(false));
                remoteInParam.Optional = true;
                remoteInParam.Description = "Remote control for Iterator, set to true to fly.";
                remoteInParam.MutableNickName = true;
                

                var remoteOutParam = this.Params.Output.Last() as Param_Boolean;
                remoteOutParam.Name = this._remoteCtrlName;
                remoteOutParam.Access = GH_ParamAccess.item;
                remoteOutParam.NickName = this._remoteCtrlName;
                //remoteOutParam.SetPersistentData(new GH_Boolean(false));
                remoteOutParam.Description = "Control downstream conponents after fly starts.";
                remoteOutParam.MutableNickName = true;
            }

            for (int i = 0; i < inputParamCount; i++)
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
            //int inputParamCount = this.Params.Input.Count - 1;
            int selectionIndex = this.Params.IndexOfInputParam(this._selectionName);

            bool isInputSide = e.ParameterSide == GH_ParameterSide.Input;
            bool isSelection = e.ParameterIndex == selectionIndex;
            bool isRemoteFly = e.Parameter.NickName == this._remoteFlyName;

            
            //check input side only
            if (!isInputSide) return;

            //check if is Selection setting only
            if (isSelection) return;

            //check if is _remoteFly setting
            if (isRemoteFly) return;
            //{
            //    //Selection's index becomes this.Params.Input.Count - 2
            //    if (e.ParameterIndex == inputParamCount-1) return;
            //}

            
            bool isSecondLastSourceFull = Params.Input[selectionIndex - 1].Sources.Any();
            // add a new input param while the second last input is full
            if (isSecondLastSourceFull)
            {
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, selectionIndex);
                Params.RegisterInputParam(newParam, selectionIndex);
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
                //todo: this can be finished inside ColibriParam
                checkInputParamNickname(sender);

                //edit the Fly output without expire this component's solution, 
                // only expire the downstream components which connected to the last output "FlyID"
                int flyIDindex = this.Params.Output.Count;
                if (this._remoteFly)
                {
                    flyIDindex = flyIDindex - 2;
                    
                }
                else
                {
                    flyIDindex = flyIDindex - 1;
                }

                this.Params.Output[flyIDindex].ExpireSolution(false);
                this.Params.Output[flyIDindex].ClearData();
                this.Params.Output[flyIDindex].AddVolatileDataList(new GH_Path(0, 0), getFlyID());



                if (_doc == null) _doc = GH.Instances.ActiveCanvas.Document;

                _doc.NewSolution(false);
                

            }
            else
            {
                sender = null;
            }
            
        }

        private List<string> getFlyID()
        {
            if (this._filteredSources.IsNullOrEmpty()) return new List<string>();

            var FlyID = new List<string>();
            foreach (var item in this._filteredSources)
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
            this._totalCount = Selections.TotalCounts;
            this._selectedCount = Selections.SelectedCounts;
            string messages = "";

            //Check selections
            checkSelections(Selections, ColibriParams, _totalCount);

            messages = "ITERATION NUMBER \nTotal: " + _totalCount;

            if (Selections.IsDefinedInSel)
            {
                messages += "\nSelected: " + _selectedCount;
                messages += "\n";
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
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The number of connected sliders must be equal to the number of items in the Steps input list.\n But Colibri will run without Division settings.");
                    
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
        
        
        //Check if Aggregator exist, and if it is at the last
        private bool isAggregatorReady()
        {
            this._aggObj = null;
            this._studyFolder = "";

            //var folder = "";
            bool isReady = true;
            
            var checkingMsg = new List<string>();

            checkingMsg = checkIfGetAggregatorObj();
            
            //Genome is not connected to Aggregator, then there is warning msg in checkingMsg 
            if (!checkingMsg.IsNullOrEmpty())
            {
                var userClick = MessageBox.Show("Colibri detected some issues. \nStill continue?\n\n\n" + checkingMsg[0], "Attention", MessageBoxButtons.YesNo);
                if (userClick == DialogResult.Yes)
                {
                    
                    return true;
                }
                else
                {
                    // user doesn't want ot continue! return false to stop
                    return false;
                }
            }
            else //this._aggObj exists
            {
                if (this._aggObj != null && this._aggObj.RuntimeMessageLevel!= GH_RuntimeMessageLevel.Error)
                {
                    //check aggregator 
                    checkingMsg = this._aggObj.CheckAggregatorIfReady();
                }
                else
                {
                    //clear _aggObj, when it has a warning runtime msg.
                    this._aggObj = null;
                    // this._aggObj doesn't exist, return true to start.
                    return true;
                }
                
            }
            
            
            // checking messages from Aggregator
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
                else
                {
                    // user click yes to ignore all Aggregator's msgs
                    return true;
                }
            }
                
            this._mode = this._aggObj.OverrideTypes;
            this._studyFolder = this._aggObj.Folder;
            return isReady;

        }
        
        //this will check two levels of components after Iteration
        private List<string> checkIfGetAggregatorObj()
        {
            
            var aggregatorID = new Guid("{c285fdce-3c5b-4701-a2ca-c4850c5aa2b7}");

            var msg = new List<string>();
            string warningMsg = "  It seems Iterator is not directly connected to Aggregator. If yes, then no pre-check and data protection.\n\t[SOLUTION]: connect Genome to Aggregator' Genome directly!";

            // only check Recipients of FlyID
            int genomeIndex = this.Params.IndexOfInputParam(this._selectionName);
            var flyIDRecipients = this.Params.Output[genomeIndex].DirectConnectedComponents();

            if (flyIDRecipients.IsNullOrEmpty()) return msg;

            var twoLevelsRecipients = new List<IGH_DocumentObject>();
            twoLevelsRecipients.AddRange(flyIDRecipients);
            foreach (var item in flyIDRecipients)
            {
                var secondLevelOutputs = new List<IGH_Param>();
                if (item is IGH_Component)
                {
                    secondLevelOutputs = ((GH_Component)item).Params.Output;
                }
                else if (item is IGH_Param)
                {
                    secondLevelOutputs.Add(item as IGH_Param);
                }
                
                foreach (var secondLevelOutput in secondLevelOutputs)
                {
                    var secondLevelRecipients = secondLevelOutput.DirectConnectedComponents();
                    twoLevelsRecipients.AddRange(secondLevelRecipients);
                }

            }
            

            foreach (var item in twoLevelsRecipients)
            {
                if (item.ComponentGuid.Equals(aggregatorID))
                {
                    this._aggObj = item as Aggregator;
                }

            }

            if (this._aggObj == null)
            {
                msg.Add(warningMsg);
            }
            
            return msg;
            
        }

        
        #endregion
        
    }



}