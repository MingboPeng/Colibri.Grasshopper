using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using GH = Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
using System.Linq;

namespace Colibri.Grasshopper
{
    public class Iterator : GH_Component
    {
        GH_Document doc = null;
        IGH_Param slidersInput;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Iterator()
          : base("Iterator", "Iterator",
              "Generates design iterations from a collection of sliders.",
              "Colibri", "Colibri")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Sliders", "S",
                "Sliders to iterate over.  Sliders must be plugged directly into this input.", GH_ParamAccess.list);
            slidersInput = pManager[0];
            pManager.AddIntegerParameter("Steps", "St", "Number of steps to take on each slider.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Fly", "F", "Tell Colibri to fly!  Provide a button here.", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Inputs", "I",
                "Colibri inputs object.  Plug this into the Colibri aggregator downstream.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //get a reference to the current doc the first time through
            if (doc == null)
            {
                doc = GH.Instances.ActiveCanvas.Document; 
            }


            //catch grasshopper inputs

            //run or no?
            bool _fly = false;
            DA.GetData(2, ref _fly);


            //output slider values and names
            List<double> sliderValues = new List<double>();
            DA.GetDataList(0, sliderValues);
            List<string> sliderNames = getConnectedSlidersNames();


            //output 'inputs' object
            Dictionary<string, double> inputs = new Dictionary<string, double>();
            for (int i = 0; i < sliderValues.Count; i++)
            {
                try
                {
                    inputs.Add(sliderNames[i], sliderValues[i]);
                }
                catch (ArgumentException ex)
                {
                    ex.ToString().Contains("key");
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error,  "Your sliders must have unique nicknames!  Set them all and try again.");
                    return;
                }
                catch (Exception ex)
                {
                    
                }
            }
            DA.SetDataList(0, inputs);


            if (!_fly)
                return;

            if (_running)
                return;

            _run = true;

            

            doc.SolutionEnd += OnSolutionEnd;
        }



        //methods below copied in from Ladybug Fly component
        private bool _run = false;
        private bool _running = false;
        //private List<System.Object> _sliders;

        GH.Kernel.Special.GH_Group grp = new GH.Kernel.Special.GH_Group();


        private void OnSolutionEnd(object sender, GH_SolutionEventArgs e)
        {
            // Unregister the event, we don't want to get called again.
            e.Document.SolutionEnd -= OnSolutionEnd;

            // If we're not supposed to run, abort now.
            if (!_run)
                return;

            // If we're already running, abort now.
            if (_running)
                return;

            // Reset run and running states.
            _run = false;
            _running = true;

            try
            {
                // Find the Guid for connected slides
                List<System.Guid> guids = new List<System.Guid>(); //empty list for guids
                GH.Kernel.IGH_Param selSlidersInput = this.Params.Input[0]; //ref for input where sliders are connected to this component
                IList<GH.Kernel.IGH_Param> sources = selSlidersInput.Sources; //list of things connected on this input
                bool isAnythingConnected = sources.Any(); //is there actually anything connected?


                // Find connected
                GH.Kernel.IGH_Param trigger = this.Params.Input[1].Sources[0]; //ref for input where a boolean or a button is connected
                GH.Kernel.Special.GH_BooleanToggle boolTrigger = trigger as GH.Kernel.Special.GH_BooleanToggle;

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
                if (sliders.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("No sliders could be found", "<harsh buzzing sound>", MessageBoxButtons.OK);
                    return;
                }

                //we now have all sliders
                //ask the user to give a sanity check
                int counter = 0;
                int totalLoops = 1;
                string popupMessage = "";

                // create progress bar by dots and |
                string pb = ".................................................."; //50 of "." - There should be a better way to create this in C# > 50 * "." does it in Python!
                char[] pbChars = pb.ToCharArray();

                foreach (GH.Kernel.Special.GH_NumberSlider slider in sliders)
                {
                    totalLoops *= (slider.TickCount + 1);
                    popupMessage += slider.ImpliedNickName;
                    popupMessage += "\n";
                }
                if (System.Windows.Forms.MessageBox.Show(sliders.Count + " slider(s) connected:\n" + popupMessage +
                  "\n" + totalLoops.ToString() + " iterations will be done. Continue?" + "\n\n (Press ESC to pause during progressing!)", "Start?", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    SetBooleanToFalse(boolTrigger);
                    this.Message = "Release the fly!";
                    return;
                }

                // Set all sliders back to first tick
                foreach (GH.Kernel.Special.GH_NumberSlider slider in sliders)
                    slider.TickValue = 0;

                //start a stopwatch
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                // Start a giant loop in which we'll permutate our way across all slider layouts.
                while (true)
                {
                    int idx = 0;

                    // let the user cancel the process
                    if (GH_Document.IsEscapeKeyDown())
                    {
                        if (System.Windows.Forms.MessageBox.Show("Do you want to stop the process?\nSo far " + counter.ToString() +
                          " out of " + totalLoops.ToString() + " iterations are done!", "Stop?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            // cancel the process by user input!
                            SetBooleanToFalse(boolTrigger);
                            this.Message += "\nCanceled by user! :|";
                            return;
                        }
                    }

                    if (!MoveToNextPermutation(ref idx, sliders))
                    {
                        // study is over!
                        SetBooleanToFalse(boolTrigger);
                        sw.Stop(); //stop start watch
                        UpdateProgressBar(counter, totalLoops, sw, pbChars);
                        this.Message += "\nFinished at " + DateTime.Now.ToShortTimeString();
                        break;
                    }

                    // We've just got a new valid permutation. Solve the new solution.
                    counter++;
                    e.Document.NewSolution(false);
                    Rhino.RhinoDoc.ActiveDoc.Views.Redraw();
                    UpdateProgressBar(counter, totalLoops, sw, pbChars);
                }
            }
            catch
            {
                // "something went wrong!";
            }
            finally
            {
                // Always make sure that _running is switched off.
                _running = false;
            }
        }

        private bool MoveToNextPermutation(ref int index, List<GH.Kernel.Special.GH_NumberSlider> sliders)
        {
            if (index >= sliders.Count)
                return false;

            GH.Kernel.Special.GH_NumberSlider slider = sliders[index];
            if (slider.TickValue < slider.TickCount)
            {
                // Increment the slider.
                slider.TickValue++;
                return true;
            }
            else
            {
                // The current slider is already at the maximum value. Reset it back to zero.
                slider.TickValue = 0;

                // Move on to the next slider.
                index++;

                // If we've run out of sliders to modify, we're done permutatin'
                if (index >= sliders.Count)
                    return false;

                return MoveToNextPermutation(ref index, sliders);
            }
        }

        private void SetBooleanToFalse(GH.Kernel.Special.GH_BooleanToggle boolTrigger)
        {
            if (boolTrigger == null) return;

            grp.Colour = System.Drawing.Color.IndianRed;
            boolTrigger.Value = false; //set trigger value to false
            boolTrigger.ExpireSolution(true);
        }

        private void UpdateProgressBar(int counter, int totalLoops, System.Diagnostics.Stopwatch sw, char[] pbChars)
        {
            // calculate percentage and update progress bar!
            double pecentageComplete = Math.Round((double)100 * (counter + 1) / totalLoops, 2);

            int lnCount = (int)pecentageComplete / (100 / pbChars.Length); //count how many lines to be changed!

            for (int i = 0; i < lnCount; i++) pbChars[i] = '|';

            string pbString = new string(pbChars);

            // format and display the TimeSpan value
            System.TimeSpan ts = sw.Elapsed;

            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
              ts.Hours, ts.Minutes, ts.Seconds,
              ts.Milliseconds / 10);

            // calculate average run time
            double avergeTime = Math.Round(ts.TotalSeconds / (counter + 1), 2); // average time for each iteration

            double expectedTime = Math.Round((ts.TotalMinutes / (counter + 1)) * totalLoops, 2); // estimation for total runs

            this.Message = elapsedTime + "\n" + pbString + "\n" + pecentageComplete.ToString() + "%\n"
              + "Average Time: " + avergeTime + " Sec.\n"
              + "Est. Total Time: " + expectedTime + " Min.\n";
        }

        private void AddToggle()
        {
            var toggle = new GH.Kernel.Special.GH_BooleanToggle();
            toggle.CreateAttributes();
            toggle.Value = false;
            toggle.NickName = "Release the fly...";
            toggle.Attributes.Pivot = new PointF((float)(this.Attributes.Bounds.Left - 200), (float)(this.Attributes.Bounds.Top + 30));
            doc.AddObject(toggle, false);
            this.Params.Input[1].AddSource(toggle);
            toggle.ExpireSolution(true);

            grp = new GH.Kernel.Special.GH_Group();
            grp.CreateAttributes();
            grp.Border = GH.Kernel.Special.GH_GroupBorder.Blob;
            grp.AddObject(toggle.InstanceGuid);
            grp.Colour = System.Drawing.Color.IndianRed;
            grp.NickName = "";
            doc.AddObject(grp, false);
        }

        private List<string> getConnectedSlidersNames()
        {
            List<string> names = new List<string>();

            // Find the Guid for connected slides
            List<System.Guid> guids = new List<System.Guid>(); //empty list for guids
            GH.Kernel.IGH_Param selSlidersInput = this.Params.Input[0]; //ref for input where sliders are connected to this component
            IList<GH.Kernel.IGH_Param> sources = selSlidersInput.Sources; //list of things connected on this input
            bool isAnythingConnected = sources.Any(); //is there actually anything connected?


            // Find connected
            GH.Kernel.IGH_Param trigger = this.Params.Input[1].Sources[0]; //ref for input where a boolean or a button is connected
            GH.Kernel.Special.GH_BooleanToggle boolTrigger = trigger as GH.Kernel.Special.GH_BooleanToggle;

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


            foreach (GH.Kernel.Special.GH_NumberSlider slider in sliders)
            {
                names.Add(slider.NickName);
            }

            return names;
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
                return Properties.Resources.Colibri_logobase_1;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{acef353c-e01e-44de-a8a6-07f93eac6a1d}"); }
        }
    }
}
