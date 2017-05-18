using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using System;
using System.Windows.Forms;
using System.IO;

namespace Colibri.Grasshopper
{
    public class IteratorFlyParam
    {

        //InputParams objects List
        private List<ColibriParam> _inputParams;

        public List<ColibriParam> InputParams
        {
            get { return _inputParams; }
            set { _inputParams = value; }
        }

        private List<string> _studiedFlyID;

        //List of each input param's all steps index
        private List<List<int>> _allPositions;
        private List<List<int>> _allSelectedPositions;
        private int _selectedCounts;
        private List<int> _iniPositions;


        //current each position
        private List<int> _currentPositionsIndex { get; set; }

        //current counts
        public static int Count { get; private set; }
       
        // Total Iteration number int
        public int _totalCounts { get; private set; }
        

        private string _watchFilePath { get; set; }
        //private string _studyFoler = String.Empty;

        private IteratorSelection _selections { get; set; }
        private OverrideMode _overrideFolderMode = OverrideMode.AskEverytime;

        private List<List<int>> iterationsFlyList = new List<List<int>>();

        //constructor 
        public IteratorFlyParam(){}

        public IteratorFlyParam(List<ColibriParam> sourceParams, IteratorSelection selections, string studyFolder, OverrideMode overrideFolderMode)
        {
            this._inputParams = sourceParams;
            this._selections = selections == null? new IteratorSelection(): selections;
            this._overrideFolderMode = overrideFolderMode;
            //this._studyFoler = studyFolder;

            this._totalCounts = ColibriBase.CalTotalCounts(this._inputParams);
            this._selectedCounts = _selections.SelectedCounts > 0 ? _selections.SelectedCounts : _totalCounts;
            this._allPositions = ColibriBase.AllParamsPositions(this._inputParams);
            this._allSelectedPositions = _selections.ParamsSelectedPositions == null? this._allPositions : _selections.ParamsSelectedPositions;
            this._currentPositionsIndex = Enumerable.Repeat(0, _inputParams.Count()).ToList();
            Count = 0;

            createWatchFile(studyFolder);

            //get studied Fly ID from folder
            if (this._overrideFolderMode == OverrideMode.FinishTheRest)
            {
                this._studiedFlyID = getStudiedFlyID(studyFolder);
            }

        }


        #region Methods
        
        //create a watch file 
        private void createWatchFile(string FolderPath)
        {
            if (!string.IsNullOrEmpty(FolderPath))
            {
                this._watchFilePath = FolderPath + "\\running.txt";
                try
                {
                    File.WriteAllText(this._watchFilePath, "running");
                }
                catch (Exception)
                {
                    throw;
                }

            }
        }
        
        private void FirstResetAll(bool isTest)
        {

            this._iniPositions = calIniPositions();

            for (int i = 0; i < InputParams.Count; i++)
            {
                if (isTest)
                {
                    InputParams[i].Reset();
                }
                else
                {
                    InputParams[i].SetParamTo(this._iniPositions[i]);
                }
                
            }
            
        }

        private List<int> calIniPositions()
        {
            var iniPositions = new List<int>();

            foreach (var item in _allSelectedPositions)
            {
                iniPositions.Add(item.First());
            }

            return iniPositions;
            
        }
        
        //public void FlyAll(GH_SolutionEventArgs e)
        //{
            
        //    FirstResetAll(false);

            
        //    while (true)
        //    {
                
        //        int currentParamIndex = 0;
        //        bool isRunning = true;
                
        //        //move to the next set of slider positions
        //        isRunning = MoveToNextPermutation(ref currentParamIndex);

                

        //        //watch the selection
        //        bool isInSelection = ifInSelectionDomains(this._selections, Count);
                

        //        if (isInSelection)
        //        {
        //            if (this._overrideFolderMode == OverrideMode.FinishTheRest)
        //            {
        //                bool isStudiedID = this._studiedFlyID.Contains(getCurrentFlyID());
        //                if (!isStudiedID)
        //                {
        //                    // We've just got a new valid permutation. Solve the new solution.
        //                    e.Document.NewSolution(false);
        //                }
        //            }
        //            else
        //            {
        //                e.Document.NewSolution(false);
        //            }
                    
        //        }

        //        Count++;

        //        //watch the user cancel the process
        //        if (GH_Document.IsEscapeKeyDown())
        //        {
        //            if (MessageBox.Show("Do you want to stop the process?\nSo far " + Count.ToString() +
        //              " out of " + this._selectedCounts.ToString() + " iterations are done!", "Stop?", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //            {
        //                // cancel the process by user input!
        //                isRunning = false;
        //            }
        //        }

        //        //watch the file to stop
        //        if (!string.IsNullOrEmpty(_watchFilePath))
        //        {
        //            if (!File.Exists(_watchFilePath))
        //            {
        //                // watch file was deleted by user
        //                isRunning = false;
        //            }
        //        }

        //        if (!isRunning )
        //        {
        //            // study is over!
                    
        //            if (File.Exists(_watchFilePath))
        //            {
        //                File.Delete(_watchFilePath);
        //            }
        //            //e.Document.NewSolution(false);
        //            break;
        //        }
                
        //    }
            
        //}

        public void FlyTest(GH_SolutionEventArgs e, int testNumber)
        {
            
            if (_totalCounts <= testNumber)
            {
                Fly(e);
                return;
            }

            FirstResetAll(true);

            int totalParamCount = _inputParams.Count;
            var random = new Random();

            var flewID = new HashSet<string>();
            
            while (true)
            {
                //pick random input param
                int randomParamIndex = random.Next(totalParamCount);
                var currentInputParam = _inputParams[randomParamIndex];

                //pick random step of param
                int randomStepIndex = random.Next(currentInputParam.TotalCount);
                var flyID = randomParamIndex + "_" + randomStepIndex;

                //test if the same flyID has already ran
                if (!flewID.Contains(flyID))
                {
                    
                    flewID.Add(flyID);
                    currentInputParam.SetParamTo(randomStepIndex);
                    Count++;

                    e.Document.NewSolution(false);

                    if (flewID.Count >= testNumber)
                    {
                        // study is over!
                        if (File.Exists(_watchFilePath))
                        {
                            File.Delete(_watchFilePath);
                        }
                        break;
                    }

                }
            }
        }
        
       
        //private bool MoveToNextPermutation(ref int MoveToParamIndex)
        //{

           
        //    if (MoveToParamIndex >= this._inputParams.Count)
        //    {
        //        return false;
        //    }

        //    //System.Windows.Forms.MessageBox.Show(Count.ToString());
            

        //    var currentParam = this._inputParams[MoveToParamIndex];
        //    var thisSelectedPositions = this._allSelectedPositions[MoveToParamIndex];
        //    //int currentStepPosition = currentStepPositionsIndex[MoveToParamIndex];

        //    int nextPositionIndex = this._currentPositionsIndex[MoveToParamIndex]+1; 
            

        //    if (nextPositionIndex < thisSelectedPositions.Count)
        //    {
        //        int nextPosition = thisSelectedPositions[nextPositionIndex];
                
        //        currentParam.SetParamTo(nextPosition);
                
        //        //Increment the current step position
        //        this._currentPositionsIndex[MoveToParamIndex]++ ;
                
        //        return true;
        //    }else
        //    {
                
        //        //currentParam.Reset();
        //        currentParam.SetParamTo(thisSelectedPositions.First());

        //        ////The current component is already at the maximum value. Reset it back to zero.
        //        this._currentPositionsIndex[MoveToParamIndex] = 0;

        //        //// Move on to the next slider.
        //        MoveToParamIndex++;

        //        //// If we've run out of sliders to modify, we're done permutatin'
        //        if (MoveToParamIndex >= _inputParams.Count)
        //        {
        //            return false;
        //        }
                    
        //        return MoveToNextPermutation(ref MoveToParamIndex);
        //    }
            
        //}

        public void Fly(GH_SolutionEventArgs e)
        {

            FirstResetAll(false);

            var flyPositions = FlyPositions(this._allSelectedPositions);
            var selflyPositions = selectedIterations(this._selections, flyPositions);

            int totalIterations = selflyPositions.Count;

            
            bool isRunning = true;
            bool stopByUser = false;
            
            foreach (var item in selflyPositions)
            {
                //set each param to target positon
                for (int i = 0; i < item.Count; i++)
                {
                    var currentParam = this._inputParams[i];
                    int moveToPosition = item[i];
                    currentParam.SetParamTo(moveToPosition);
                }

                
                if (!checkIfStudiedFromCSV())
                {
                    e.Document.NewSolution(false);
                }



                Count++;
                isRunning = Count < totalIterations;
                stopByUser = !listenToKeepRunning();


                if (!isRunning || stopByUser)
                {
                    // study is over!
                    if (File.Exists(_watchFilePath))
                    {
                        File.Delete(_watchFilePath);
                    }
                    break;
                }


                
            }

        }

        private bool listenToKeepRunning()
        {
            //watch the user cancel the process
            if (GH_Document.IsEscapeKeyDown())
            {
                if (MessageBox.Show("Do you want to stop the process?\nSo far " + Count.ToString() +
                  " out of " + this._selectedCounts.ToString() + " iterations are done!", "Stop?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // cancel the process by user input!
                    return false;
                }
            }

            //watch the file to stop
            if (!string.IsNullOrEmpty(_watchFilePath))
            {
                if (!File.Exists(_watchFilePath))
                {
                    // watch file was deleted by user
                    return false;
                }
            }

            return true;
        }

        //from Grasshopper's CrossReference
        private bool nextPosition(List<int> offsets, List<int> lengths, int index)
        {
            if (index >= offsets.Count)
                return false;
            if (lengths[index] < 0)
                return this.nextPosition(offsets, lengths, checked(index + 1));
            if (offsets[index] >= lengths[index])
            {
                offsets[index] = 0;
                return this.nextPosition(offsets, lengths, checked(index + 1));
            }
            List<int> intList1 = offsets;
            List<int> intList2 = intList1;
            int index1 = index;
            int index2 = index1;
            int num = checked(intList1[index1] + 1);
            intList2[index2] = num;
            return true;
        }

        private List<List<int>> FlyPositions(List<List<int>> inLists)
        {
            List<List<int>> inputLists = inLists;
            List<List<int>> crossReferencedList = new List<List<int>>();
            List<List<int>> positionsOfEachIterations = new List<List<int>>();
            List<int> lengths = new List<int>();
            List<int> offsets = new List<int>();
            List<int> intList = new List<int>();

            for (int i = 0; i < inputLists.Count; i++)
            {
                if (inputLists[i].Count != 0)
                {
                    intList.Add(i);
                    offsets.Add(0);
                    lengths.Add(checked(inputLists[i].Count - 1));
                    crossReferencedList.Add(new List<int>());
                }
            }
            

            if (offsets.Count == 0)
                return new List<List<int>>();

            offsets[0] = -1;

            while (this.nextPosition(offsets, lengths, 0))
            {
                int num10 = 0;
                int num11 = checked(inputLists.Count - 1);
                int index8 = num10;
                while (index8 <= num11)
                {
                    int index3 = offsets[index8];

                    if (index3 <= lengths[index8])
                    {
                        crossReferencedList[index8].Add(inputLists[index8][index3]);
                    }
                    checked { ++index8; }
                }
            }

            int iterationCount = crossReferencedList[0].Count;
            int paramCount = crossReferencedList.Count;

            for (int i = 0; i < iterationCount; i++)
            {
                positionsOfEachIterations.Add(new List<int>());
                for (int j = 0; j < paramCount; j++)
                {
                    int setIndex = crossReferencedList[j][i];
                    positionsOfEachIterations.Last().Add(setIndex);
                }
            }

            return positionsOfEachIterations;
        }

        private List<List<int>> selectedIterations(IteratorSelection Selections, List<List<int>> allIterations)
        {
            int count = 0;
            bool ifInSelection = true;
            var selectedIterationPositions = new List<List<int>>();
        
            foreach (var item in allIterations)
            {
                
                ifInSelection = ifInSelectionDomains(Selections, count);
                if (ifInSelection)
                {
                    selectedIterationPositions.Add(allIterations[count]);
                }
                count++;
            }
            
            return selectedIterationPositions;
            


        }

        private bool ifInSelectionDomains(IteratorSelection Selections, int CurrentCount)
        {
            //Selections undefined, so all is in seleciton
            if (!Selections.IsDefinedInSel)
            {
                return true;
            }

            var currentCount = (double)CurrentCount;
            bool isInSelection = true;
            int includedCounts = 0;

            if (Selections.Domains.Any())
            {
                foreach (var domain in Selections.Domains)
                {
                    
                    includedCounts += domain.Value.IncludesParameter(currentCount) == true ? 1 : 0;
                }
            }

            isInSelection = includedCounts == 0 ? false : true;
            return isInSelection;

        }

        private string getCurrentFlyID()
        {
            var inputParams = this._inputParams;
            string currentNickname = String.Empty;
            string currentValue = String.Empty;
            string flyID = String.Empty;

            var currentValues = new List<string>();
            for (int i = 0; i < inputParams.Count; i++)
            {
                currentValue = inputParams[i].CurrentValue();
                currentValues.Add(currentValue);
            }

            flyID = string.Join("_", currentValues);

            return flyID;
        }

        private List<string> getStudiedFlyID(string FolderPath)
        {
            var inputParams = this._inputParams;
            string csvFilePath = FolderPath + @"\data.csv";
            if (!File.Exists(csvFilePath)) return new List<string>();
            var stringLines = File.ReadAllLines(csvFilePath);
            var studiedFlyID = new List<string>();
            
            for (int i = 0; i < stringLines.Count(); i++)
            {
                var itemValues = stringLines[i].Split(',').Take(inputParams.Count);
                string oneID = string.Join("_",itemValues);
                studiedFlyID.Add(oneID);
            }

            return studiedFlyID;

        }
        
        private bool checkIfStudiedFromCSV()
        {
            if (this._overrideFolderMode == OverrideMode.FinishTheRest)
            {
                return this._studiedFlyID.Contains(getCurrentFlyID());
            }
            else
            {
                return false;
            }
            
        }


        #endregion

    }

}
