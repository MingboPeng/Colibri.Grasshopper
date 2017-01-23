using System;
using System.Drawing;
using System.Collections.Generic;
using Grasshopper.Kernel.Special;
using System.IO;
using GH = Grasshopper;
using Grasshopper.Kernel;
using System.Windows.Forms;

namespace Colibri.Grasshopper
{
    
    public class Aggregator : GH_Component
    {
        GH_Document doc = null;
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Aggregator()
          : base("Aggregator", "Aggregator",
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
            pManager.AddTextParameter("Inputs", "Inputs", "Inputs object from the Colibri Iterator compnent.", GH_ParamAccess.list);
            pManager.AddTextParameter("Outputs", "Outputs", "Outputs object from the Colibri Outputs component.", GH_ParamAccess.list);
            pManager.AddTextParameter("ImgParams", "ImgParams", "ImgParams object from the Colibri ImageParameters component.", GH_ParamAccess.list);
            pManager[3].Optional = true;
            pManager.AddBooleanParameter("Write?", "Write?", "Set to true to write files to disk.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("SpectaclesFileName", "SpectaclesFileName",
                "Feed this into the Spectacles_SceneCompiler component downstream.", GH_ParamAccess.item);

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
            string folder = "";
            List<string> inputs = new List<string>();
            List<string> outputs = new List<string>();
            List<string> imgParams = new List<string>();
            bool writeFile = false;
            
            //get data
            DA.GetData(0, ref folder);
            DA.GetDataList(1, inputs);
            DA.GetDataList(2, outputs);
            DA.GetDataList(3, imgParams);
            DA.GetData(4, ref writeFile);

            //operations
            Dictionary<string,string> inputCSVstrings = ColibriBase.FormatDataToCSVstring(inputs,"in:");
            Dictionary<string, string> outputCSVstrings = ColibriBase.FormatDataToCSVstring(outputs,"out:");
            Dictionary<string, string> imgParamsClean = ColibriBase.ConvertBactToDictionary(imgParams);




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

            
            int width = 400;
            int height = 400;

            
            bool run = writeFile;
            //string fileName = imgName;
            string imgName = flyID;
            
            
            string writeInData = "";

            // overwrite the image parameter setting if user has inputed the values
            if (imgParamsClean.Count > 0)
            {
                bool isThereNoImgName = String.IsNullOrEmpty(imgParamsClean["imgName"]) || imgParamsClean["imgName"] == "defaultName";
                imgName = isThereNoImgName ? imgName : imgParamsClean["imgName"];
                width = Convert.ToInt32(imgParamsClean["Width"]);
                height = Convert.ToInt32(imgParamsClean["Height"]);
            }
            
            Size viewSize = new Size(width, height);

            string imgFileName = imgName + ".png";
            string imgPath = folder + @"\" + imgFileName;

            string jsonFileName = imgName + ".json";
            string jsonFilePath = folder + @"\" + jsonFileName;


            //if we aren't told to write, clean out the list of already written items
            if (!run)
            {
                alreadyWrittenLines = new List<string>();
            }

            
            
                //if we are told to run and we haven't written this line yet, do so

            if (run && !alreadyWrittenLines.Contains(valueReady))
            {
                //Check if Aggregator is at the calculation list end
                //var userClick = DialogResult.No;
                if (!isGoodToSeeAllView())
                {
                    var userClick = MessageBox.Show("Aggregator might not capture all objects that you can see in Rhino view. \nStill continue?" + "\n\n (Click No, select Aggregator and press Ctrl+F can save your life!)", "Attention", MessageBoxButtons.YesNo);
                    if (userClick == DialogResult.No)
                    {
                        var toggle = this.Params.Input[4].Sources[0] as GH_BooleanToggle;
                        if (toggle!= null)
                        {
                            toggle.Value = false; //set trigger value to false
                            toggle.ExpireSolution(true);
                        }
                        
                        return;
                    }
                }

                
                //add this line to our list of already written lines
                alreadyWrittenLines.Add(valueReady);

                //Check folder if existed
                Directory.CreateDirectory(folder);
                

                //check csv file
                if (!File.Exists(csvPath))
                {
                    keyReady = keyReady + ",img,threeD" + Environment.NewLine;
                    File.WriteAllText(csvPath, keyReady);
                }

                //save imgs
                Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.Redraw();
                var pic = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.CaptureToBitmap(viewSize);
                pic.Save(imgPath);

                //save csv
                writeInData = string.Format("{0},{1},{2}\n", valueReady, imgFileName, jsonFileName);
                File.AppendAllText(csvPath, writeInData);
                
            }
            
            //set output
            //DA.SetData(0, writeInData);
            DA.SetData(0, jsonFilePath);

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



        public bool isGoodToSeeAllView()
        {
            if (doc == null)
            {
                doc = GH.Instances.ActiveCanvas.Document;
            }
            int totalObjs = doc.ObjectCount;
            int executePosition = 0;
            //var runList = new List<string>();

            //foreach (IGH_DocumentObject obj in doc.Objects)
            //{
            //    runList.Add(obj.NickName + " (" + obj.Name + ")" + " (" + obj.Category + ")");
            //}
            for (int i = 0; i < totalObjs; i++)
            {
                //bool isDisplayTypeObj = doc.Objects[i].Category == "Display";
                //bool isNamedPreview = doc.Objects[i].Name.Contains("Preview");
                //bool isWireframe = doc.Objects[i].Name.Contains("Wireframe");
                bool isAggregator = doc.Objects[i].ComponentGuid.Equals(new Guid("{787196c8-5cc8-46f5-b253-4e63d8d271e1}"));
                if (isAggregator)
                {
                    executePosition = i;
                }
            }


            if (executePosition == totalObjs-1)
            {
                //Aggregator is at the last position to be execulted, so can capture all previewed objs
                return true;
            }
            else
            {
                return false;
            }
            
        }


    }
}
