using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.Grasshopper
{
    //this class is created for containing Slider, Pannel, ValueList
    public class ColibriParam
    {
        //Properties
        public InputType GHType { get; private set; }


        private string nickName;

        public string NickName
        {
            get
            {
                if (nickName != Param.NickName)
                {
                    nickName = Param.NickName;
                }
                return nickName;
            }
            set
            {
                nickName = value;
                Param.NickName = nickName;
            }
        }


        //todo: merge this to position
        // for now is used for tracking the panel values positon only
        //private int panelItemPosition;

        private int _position;

        public int Position
        {
            get { return _position; }
            private set { _position = value; }
        }

        public int AtIteratorPosition { get; set; }
        private int totalCount;

        public int TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }

        private List<string> panelValues = new List<string>();

        public IGH_Param Param { get; private set; }

        //for flyParam
        private int _iniPosition;

        public int IniPosition
        {
            get { return _iniPosition; }
            set { _iniPosition = value; }
        }

        //Constructor
        public ColibriParam()
        {

        }
        public ColibriParam(IGH_Param RawParam, int atIteratorPosition)
        {
            Param = RawParam;
            AtIteratorPosition = atIteratorPosition;


            GHType = GetGHType();
            nickName = Param.NickName;
            
            if (GHType == InputType.Panel)
            {
                var panel = this.Param as GH_Panel;
                panelValues = getPanelValue(panel);

            }

            TotalCount = CountSteps();

            //check slider's Implied Nickname
            if (GHType == InputType.Slider)
            {
                var slider = this.Param as GH_NumberSlider;
                nickName = String.IsNullOrEmpty(slider.NickName) && slider.ImpliedNickName !="Input"? slider.ImpliedNickName: slider.NickName;
                slider.NickName = nickName;
            }

            CalIniPosition();


        }
        
        //Methods
        private InputType GetGHType()
        {
            var rawParam = this.Param;
            //Check raw param if is null first
            if (rawParam == null)
            {
                return InputType.Unsupported;
            }
            else if (rawParam is GH_NumberSlider)
            {
                return InputType.Slider;

            }
            else if (rawParam is GH_Panel)
            {
                return InputType.Panel;
            }
            else if (rawParam is GH_ValueList)
            {
                return InputType.ValueList;
            }
            else
            {
                return InputType.Unsupported;
            }
        }

        private List<string> getPanelValue(GH_Panel panel)
        {
            var panelValues = new List<string>();
            //var panelVolatileValues = new List<string>();

            //panelValues = panel.UserText.Split('\n').ToList();
            panelValues = panel.VolatileData.AllData(true).Select(_ => _.ToString()).ToList();

            //if (panelValues.Count ==1 && panelValues[0].Contains("Double click to edit panel content"))
            //{
            //    panelValues = panelVolatileValues;
            //}
            //else
            //{

            //}
            
            return panelValues;
        }


        public string CurrentValue() {

            var rawParam = this.Param;
            string currentValue = string.Empty;

            if (GHType == InputType.Slider)
            {
                var slider = rawParam as GH_NumberSlider;
                currentValue = slider.CurrentValue.ToString();
                
            }
            else if (GHType == InputType.Panel)
            {
                panelValues = getPanelValue(rawParam as GH_Panel);
                currentValue = panelValues[_position];
            }
            else if (GHType == InputType.ValueList)
            {
                //todo: two mode for running or unrunning 
                var valueList = rawParam as GH_ValueList;
                currentValue = valueList.FirstSelectedItem.Value.ToString();
                //currentValue = valueList.ListItems[position].Value.ToString();
            }
            else
            {
                currentValue = "Please use Slider, Panel, or ValueList!";
            }

            return currentValue;

        }
        public string CurrentValue(bool isFlying)
        {

            var rawParam = this.Param;
            string currentValue = string.Empty;

            if (GHType == InputType.Slider)
            {
                var slider = rawParam as GH_NumberSlider;
                currentValue = slider.CurrentValue.ToString();

            }
            else if (GHType == InputType.Panel)
            {
                currentValue = panelValues[_position];

            }
            else if (GHType == InputType.ValueList)
            {
                //two modes for running or unrunning 
                var valueList = rawParam as GH_ValueList;
                if (isFlying)
                {
                    currentValue = valueList.ListItems[_position].Value.ToString();
                }
                else
                {
                    currentValue = valueList.FirstSelectedItem.Value.ToString();
                    
                }
                
            }
            else
            {
                currentValue = "Please use Slider, Panel, or ValueList!";
            }

            return currentValue;

        }
        private void CalIniPosition()
        {

            var rawParam = this.Param;
            int position = this._position;


            if (GHType == InputType.Slider)
            {
                var slider = rawParam as GH_NumberSlider;
                position = slider.TickValue;
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = rawParam as GH_ValueList;
                for (int i = 0; i < valueList.ListItems.Count(); i++)
                {
                    var item = valueList.ListItems[i];
                    if (item.Selected)
                    {
                        position = i;
                    }
                }
            }
            else
            {
                position = 0;
            }

            this._position = position;

        }

        private int CountSteps()
        {
            
            var param = this.Param;
            var count = 0;


            //Slider
            if (GHType == InputType.Slider)
            {
                var mySlider = param as GH_NumberSlider;
                count = mySlider.TickCount + 1;

            }

            //Panel
            else if (GHType == InputType.Panel)
            {
                
                count = panelValues.Count();

            }

            //ValueList
            else if (GHType == InputType.ValueList)
            {
                var myValueList = param as GH_ValueList;

                count = myValueList.ListItems.Count();
                
            }
            else
            {
                count = 0;
            }


            return count;
        }

        public void SetParamTo(int SetToStepIndex)
        {

            var param = this.Param;
            
            this._position = SetToStepIndex;

            if (GHType == InputType.Slider)
            {
                var slider = param as GH_NumberSlider;
                slider.TickValue = _position;

            }
            else if (GHType == InputType.Panel)
            {
                this.Param.ExpireSolution(false);
            }
            else if (GHType == InputType.ValueList)
            {
                //var valueList = param as GH_ValueList;
                //valueList.SelectItem(position);
                //valueList.ToggleItem(SetToStepIndex);
                
                //this.Param.ExpireSolution(false);


                var valueList = param as GH_ValueList;
                string state = indexToValueListState(_position);
                valueList.LoadState(state);
                this.Param.ExpireSolution(false);

            }


        }

        // todo: SetToNext() 
        public void SetToNext()
        {
            var param = this.Param;
            
            if (GHType == InputType.Slider)
            {
                var slider = param as GH_NumberSlider;
                slider.TickValue ++;
                //slider.ExpireSolution(true);

            }
            else if (GHType == InputType.Panel)
            {
                _position++;
                this.Param.ExpireSolution(false);
            }
            else if (GHType == InputType.ValueList)
            {
                var valueList = param as GH_ValueList;
                //valueList.SelectItem(SetToStepIndex);
                //valueList.ToggleItem(SetToStepIndex);
                _position++;
                valueList.SelectItem(_position);

                
                //this.Param.ExpireSolution(false);

            }
        }
        public void Reset()
        {
            SetParamTo(0);
        }
        
        //this convert current step position to ValueList state string.
        private string indexToValueListState(int positionIndex)
        {
            int position = positionIndex < totalCount ? positionIndex : 0;
            string state = new String('N', totalCount-1);
            state = state.Substring(0, position) + "Y" + state.Substring(position);
            return state;

        }



        // Override method
        public override string ToString()
        {
            string currentValue =CurrentValue();

            return currentValue;
        }

        public string ToString (bool withNames)
        {

            string currentValue = "[" +nickName+","+ CurrentValue()+ "]";

            return currentValue;
        }

        
    }
    
}
