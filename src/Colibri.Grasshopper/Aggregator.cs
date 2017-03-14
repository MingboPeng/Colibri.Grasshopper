using System;
using System.Drawing;
using System.Collections.Generic;
using Grasshopper.Kernel.Special;
using System.IO;
using GH = Grasshopper;
using Grasshopper.Kernel;
using System.Windows.Forms;
using System.Linq;

namespace Colibri.Grasshopper
{
    
    public class Aggregator : GH_Component
    {
        bool writeFile = false;
        public string folder = "";

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Aggregator()
          : base("Colibri Aggregator", "Aggregator",
              "Aggregates design input and output data, image & Spectacles filemanes into a data.csv file that Design Explorer can open.",
              "TT Toolbox", "Colibri")
        {
        }

        public override GH_Exposure Exposure { get {return GH_Exposure.tertiary;} }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Folder", "Folder", "Path to a directory to write images, spectacles models, and the data.csv file into.", GH_ParamAccess.item);
            pManager.AddTextParameter("FlyID(Inputs)", "FlyID", "Inputs object from the Colibri Iterator compnent.", GH_ParamAccess.list);
            pManager.AddTextParameter("FlyResults(Outputs)", "FlyResults", "Outputs object from the Colibri Outputs component.", GH_ParamAccess.list);
            pManager.AddGenericParameter("ImgParams", "ImgParams", "Optional input from the Colibri ImageParameters component.", GH_ParamAccess.item);
            pManager[3].Optional = true;
            pManager[3].WireDisplay = GH_ParamWireDisplay.faint;
            pManager.AddGenericParameter("3DParams", "3DParams", "Optional input from the Colibri 3DParameters component.", GH_ParamAccess.item);
            pManager[4].Optional = true;
            pManager[4].WireDisplay = GH_ParamWireDisplay.faint;
            pManager.AddBooleanParameter("Write?", "Write?", "Set to true to write files to disk.", GH_ParamAccess.item,false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "ReadMe",
                "...", GH_ParamAccess.item);

        }

        //variable to keep track of what lines have been written during a colibri flight
        private List<string> alreadyWrittenLines = new List<string>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input variables
            
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();
            //List<string> imgParams = new List<string>();
            var JSON = new threeDParam();
            var imgParams = new ImgParam();

            //get data
            DA.GetData(0, ref folder);
            DA.GetDataList(1, inputs);
            DA.GetDataList(2, outputs);
            DA.GetData(3, ref imgParams);
            DA.GetData(4, ref JSON);
            DA.GetData(5, ref writeFile);

            //operations
            Dictionary<string,string> inputCSVstrings = ColibriBase.FormatDataToCSVstring(inputs,"in:");
            Dictionary<string, string> outputCSVstrings = ColibriBase.FormatDataToCSVstring(outputs,"out:");
            //Dictionary<string, string> imgParamsClean = ColibriBase.ConvertBactToDictionary(imgParams);
            
            string csvPath = folder + "/data.csv";
            var rawData = inputs;
            int inDataLength = rawData.Count;
            rawData.AddRange(outputs);
            int allDataLength = rawData.Count;

            string flyID = String.Empty;
            string keyReady = String.Empty;
            string valueReady = String.Empty;
            
            //Parsing data to csv format
            keyReady = inputCSVstrings["DataTitle"] + "," + outputCSVstrings["DataTitle"];
            valueReady = inputCSVstrings["DataValue"] + "," + outputCSVstrings["DataValue"];
            flyID = inputCSVstrings["FlyID"];

            
            bool run = writeFile;
            
            
            string writeInData = "";
            //var ViewNames = new List<string>();
            

            

            //if we aren't told to write, clean out the list of already written items
            if (!run)
            {
                alreadyWrittenLines = new List<string>();
            }
            
                //if we are told to run and we haven't written this line yet, do so

            if (run && !alreadyWrittenLines.Contains(valueReady))
            {
                
                //add this line to our list of already written lines
                alreadyWrittenLines.Add(valueReady);

                //Check folder if existed
                Directory.CreateDirectory(folder);

                //save img
                keyReady += ",img";
                string imgFileName = captureViews(imgParams, flyID);

                //save json
                string jsonFileName = string.Empty;
                string jsonFilePath = string.Empty;
                if (JSON.IsDefined)
                {
                    keyReady += ",threeD";
                    jsonFileName = flyID + ".json";
                    jsonFilePath = folder + @"\" + jsonFileName;
                    File.WriteAllText(jsonFilePath, JSON.JsonSting);
                }

                //check csv file
                if (!File.Exists(csvPath))
                {
                    keyReady += Environment.NewLine;
                    File.WriteAllText(csvPath, keyReady);
                }

                //save csv
                writeInData = string.Format("{0},{1},{2}\n", valueReady, imgFileName, jsonFileName);
                File.AppendAllText(csvPath, writeInData);
                
            }
            
            //set output
            DA.SetData(0, writeInData);

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
                return Colibri.Grasshopper.Properties.Resources.Aggregator;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{787196c8-5cc8-46f5-b253-4e63d8d271e1}"); }
        }

        private string captureViews(ImgParam imgParams,string flyID)
        {
            //string imgID = flyID;
            var ViewNames = new List<string>();
            int width = 400;
            int height = 400;

            Size imageSize = new Size(width, height);
            string imgName = flyID;
            string imgPath = string.Empty;


            // overwrite the image parameter setting if user has inputed the values
            if (imgParams.IsDefined)
            {
                bool isThereNoImgName = imgParams.SaveName == "defaultName";
                imgName = isThereNoImgName ? imgName : imgParams.SaveName;
                ViewNames = imgParams.ViewNames;
                width = imgParams.Width;
                height = imgParams.Height;

            }


            //If ViewNames is empty, which means to capture current active view
            var activeView = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView;
            if (!ViewNames.Any())
            {
                imgName += ".png";
                imgPath = folder + @"\" + imgName;

                activeView.Redraw();
                var pic = activeView.CaptureToBitmap(imageSize);
                pic.Save(imgPath);

                //return here, and skip the following views' check
                return imgName;

            }
            
            //If user set View Names
            var views = Rhino.RhinoDoc.ActiveDoc.Views.ToDictionary(v => v.ActiveViewport.Name, v => v);
            var namedViews = Rhino.RhinoDoc.ActiveDoc.NamedViews.ToDictionary(v => v.Name, v => v);

            //string newImgPathWithViewName = ImagePath;

            for (int i = 0; i < ViewNames.Count; i++)
            {
                string viewName = ViewNames[i];
                string existViewName = string.Empty;

                if (views.ContainsKey(viewName))
                {
                    activeView = views[viewName];
                    existViewName = viewName;
                }
                else if(namedViews.ContainsKey(viewName))
                {
                    existViewName = viewName;
                    var namedViewIndex = Rhino.RhinoDoc.ActiveDoc.NamedViews.FindByName(viewName);
                    Rhino.RhinoDoc.ActiveDoc.NamedViews.Restore(namedViewIndex, Rhino.RhinoDoc.ActiveDoc.Views.ActiveView, true);
                }
                
                //capture
                if (!string.IsNullOrEmpty(existViewName))
                {
                    imgName += "_" + existViewName + ".png";
                    imgPath = folder + @"\" + imgName;
                    //save imgs
                    activeView.Redraw();
                    var pic = activeView.CaptureToBitmap(imageSize);
                    pic.Save(imgPath);
                    
                }

            }
            return imgName;
            
        }

        public void setWriteFileToFalse()
        {
            
            if (this.Params.Input.Last().Sources.Any())
            {
                var writeFile = this.Params.Input.Last().Sources.First() as GH_BooleanToggle;
                writeFile.Value = false;
                writeFile.ExpireSolution(true);
            }
        }



        //Check if Aggregator exist, and if it is at the last
        public List<string> CheckAggregatorIfReady()
        {
            
            var checkingMsg = new List<string>();
            checkingMsg = checkIfRecording(checkingMsg);
            checkingMsg = checkIfLast(checkingMsg);
            return checkingMsg;

        }
        
        private List<string> checkIfLast(List<string> msg)
        {
            string warningMsg = "  Aggregator might not capture all objects that you see in Rhino view.\n\t[SOLUTION]: select Aggregator and press Ctrl+F can save your life!";
            var doc = GH.Instances.ActiveCanvas.Document;
            bool isAggregatorLast = doc.Objects.Last().InstanceGuid.Equals(this.InstanceGuid);

            if (!isAggregatorLast)
            {
                msg.Add(warningMsg);
            }

            return msg;
        }

        private List<string> checkIfRecording(List<string> msg)
        {
            string warningMsg = "  Aggregator is not writing the data.\n\t[SOLUTION]: set Aggregator's \"write?\" to true.";
            var isRecording = this.Params.Input.Last().VolatileData.AllData(true).First() as GH.Kernel.Types.GH_Boolean;

            if (!isRecording.Value)
            {
                msg.Add(warningMsg);
            }

            return msg;

        }


    }
}
