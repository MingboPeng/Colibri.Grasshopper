using System;
using System.Linq;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace Colibri.Grasshopper
{
    public class ColibriOutputs : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// ne	w tabs/panels will automatically be created.
        /// </summary>
        public ColibriOutputs()
          : base("Colibri Parameters", "Parameters",
              "Collects design parameters (us engineer types would call these 'performance metrics') to chart in Design Explorer.  These will be the vertical axes to the far right on the parallel coordinates plot, next to the design inputs. These values should describe the characteristics of a single design iteration.\nYou can also combine this output as a static gene in Genome.",
              "TT Toolbox", "Colibri 2.0")
        {
            Params.ParameterSourcesChanged += ParamSourcesChanged;
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Data", "Data[1]", "Design results (performance metrics) to chart in Design Explorer.\nOne or a list of values is acceptable, but each grip is limited with 10 values max.\nNull or Empty value will be marked as \"NoData\"", GH_ParamAccess.list);
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Parameters", "Parameters", "Colibri's Parameters.  Plug this into the Colibri Aggregator downstream.  Most users should connect 'Parameters' to the 'Phenome' inout on the Aggregator.\n\nHowever, you can use 'Parameters' as an input for the Colibri Aggregator's Genome input, too, which is useful when working with Galapagos or Octopus.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //checkInputParamNickname();
            this.Message= updateComponentMsg();
            //set output data
            DA.SetDataList(0, getFlyResults());

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.Output;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{f8903fbe-3986-46e4-b588-9bddd6d5a5d0}"); }
        }


        #region Methods of IGH_VariableParameterComponent interface

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            bool isInputSide = (side == GH_ParameterSide.Input) ? true : false;
            bool isTheOnlyInput = (index == 0) ? true : false;

            //We only let input parameters to be added (output number is fixed at one)
            if (isInputSide  && !isTheOnlyInput)
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
            bool isInputSide = (side == GH_ParameterSide.Input) ? true : false;
            bool isTheOnlyInput = this.Params.Input.Count == 1 ? true : false;


            //can only remove from the input and non Fly? or the first Slider
            if (isInputSide && !isTheOnlyInput)
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
            //var outParam = new Param_GenericObject();
            //outParam.NickName = String.Empty;
            //Params.RegisterOutputParam(outParam, index);

            var param = new Param_GenericObject();
            param.NickName = String.Empty;

            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            //unregister ther output when input is destroied.
            //Params.UnregisterOutputParameter(Params.Output[index]);
            
            return true;
        }


        //Todo remove unnecessary code here
        public void VariableParameterMaintenance()
        {

            for (int i = 0; i < this.Params.Input.Count; i++)
            {
                // create inputs
                var inParam = this.Params.Input[i];
                string inParamNickname = inParam.NickName;
                int inputIndex = i + 1;

                bool isNicknameEmpty = String.IsNullOrWhiteSpace(inParamNickname);
                bool isNicknameDefault = inParamNickname.StartsWith("Data[");


                if (isNicknameEmpty|| isNicknameDefault)
                {
                    inParam.NickName = "Data[" + inputIndex+"]";
                }
                inParam.Name = "Data";
                inParam.Description = "Design results (performance metrics) to chart in Design Explorer.\nOne or a list of values is acceptable, but each grip is limited with 10 values max.\nNull or Empty value will be marked as \"NoData\"";
                inParam.Access = GH_ParamAccess.list;
                inParam.Optional = true;
                inParam.DataMapping = GH_DataMapping.Flatten;
                inParam.WireDisplay = GH_ParamWireDisplay.faint;
                
            }

        }


        #endregion

        //This is for if any source connected, reconnected, removed, replacement 
        private void ParamSourcesChanged(Object sender, GH_ParamServerEventArgs e)
        {
            
            bool isInputSide = e.ParameterSide == GH_ParameterSide.Input ? true : false;
            
            //check input side only
            if (!isInputSide) return;
            
            bool isLastSourceFull = Params.Input.Last().Sources.Any();
            // add a new input param while the second last input is full
            if (isLastSourceFull)
            {
                IGH_Param newParam = CreateParameter(GH_ParameterSide.Input, Params.Input.Count);
                Params.RegisterInputParam(newParam, Params.Input.Count);
                VariableParameterMaintenance();
                this.Params.OnParametersChanged();
            }

            ////recollecting the filteredSources and rename while any source changed
            //_filteredSources = gatherSources();
            //checkAllNames(filteredSources);
            checkInputParamNickname(e.Parameter);

        }

        private void checkInputParamNickname(IGH_Param sender)
        {
            //var allInputParams = this.Params.Input;
            //for (int i = 0; i < allInputParams.Count; i++)
            //{
            if (sender.Sources.Any())
            {
                //get nickname from source
                string inputName = sender.Sources.First().NickName;

                //if nickname is empty, then use defaultname
                if (!String.IsNullOrEmpty(inputName))
                {
                //set inputParam's nickname
                sender.NickName = inputName;
                }
                sender.ObjectChanged += InputParam_ObjectNicknameChanged;
            }
            else
            {
                sender.ObjectChanged -= InputParam_ObjectNicknameChanged;
            }
            
            this.Attributes.ExpireLayout();
            

        }


        private void InputParam_ObjectNicknameChanged(IGH_DocumentObject sender, GH_ObjectChangedEventArgs e)
        {
            if (e.Type == GH_ObjectEventType.NickName)
            {
                this.Message = updateComponentMsg();
                //edit the Fly output without expire this component's solution, 
                // only expire the downstream components which connected to the last output "FlyID"
                this.Params.Output.Last().ExpireSolution(false);
                this.Params.Output.Last().ClearData();
                this.Params.Output.Last().AddVolatileDataList(new GH_Path(0), getFlyResults());
                
                var doc = Instances.ActiveCanvas.Document;

                doc.NewSolution(false);
            }
        }
        private string updateComponentMsg()
        {
            string messages = "";
            var flyResults = getFlyResults();

            if (flyResults.Any())
            {
                messages += "[NAME,DATA]";
                messages += "\n-------------------\n";
            }
            foreach (var item in flyResults)
            {
                messages += item+"\n";
            }
            return messages;

        }

        private List<string> getFlyResults()
        {

            var FlyResults = new List<string>();
            var allInputParams = this.Params.Input;
            foreach (var item in allInputParams)
            {
                if (item.SourceCount <= 0)
                {
                    continue;
                }
                

                string nickname = item.NickName.Replace(',',' ').Replace('.', ' ');
                var values = new List<IGH_Goo>() { new GH_String("-999")};
                //this is for cases those "data not collected"
                if (!item.VolatileData.IsEmpty)
                {
                    values = item.VolatileData.AllData(false).ToList();
                }
                
                int maxNumberTake = values.Count <= 10 ? values.Count : 10;

                for (int i = 0; i < maxNumberTake; i++)
                {
                    //check Nickname
                    string currentValue = "";

                    if ((values[i] == null) || String.IsNullOrWhiteSpace( values[i].ToString()))
                    {
                        currentValue = "-999";//not data (<null> or <empty>)
                    }
                    else
                    {
                        currentValue = values[i].ToString();
                    }



                    //check Nickname
                    var currentNickname = nickname;
                    if (values.Count>1)
                    {
                        currentNickname = nickname +"_" +(i+1).ToString();
                    }

                    string resultString = "[" + currentNickname + "," + currentValue + "]";
                    FlyResults.Add(resultString);
                }
            }

            return FlyResults;
        }
    }
}