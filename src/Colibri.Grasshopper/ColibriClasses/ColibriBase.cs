using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.Grasshopper
{
    public enum InputType { Slider, Panel, ValueList, Unsupported }

    static class ColibriBase
    {
        public static Dictionary<string, string> ConvertBactToDictionary (List<string> StringLikeDictionary)
        {
            var newDictionary = new Dictionary<string, string>();

            foreach (var item in StringLikeDictionary)
            {
                var cleanItem = item.ToString().Replace(" ", "");
                cleanItem = cleanItem.Substring(1, cleanItem.Length - 2);
				var cleanItemParts = cleanItem.Split(',');

                string dicKey = cleanItemParts[0];
                string dicValue = cleanItemParts[1];
				
                if (!String.IsNullOrEmpty(dicKey))
                {
                    newDictionary.Add(dicKey, dicValue);
                }
                
            }

            return newDictionary;
        }

		public static Dictionary<string, string> FormatDataToCSVstring(List<string> StringLikeDictionary, string Prefix)
        {
            string newCSVstring = string.Empty;
            var convertedDictionary = ConvertBactToDictionary(StringLikeDictionary);

            var DesignExplorerData = new Dictionary<string , string>();

            string dataKey = string.Empty;
            string dataValue = string.Empty;
            string flyID = string.Empty;
            foreach (var item in convertedDictionary)
            {
                dataKey = String.IsNullOrEmpty(dataKey) ? Prefix + item.Key : dataKey + "," + Prefix + item.Key;
				dataValue = String.IsNullOrEmpty(dataValue) ? item.Value : dataValue + "," + item.Value;
                flyID = String.IsNullOrEmpty(flyID) ? item.Key + "_" + item.Value : flyID + "_" + item.Key + "_" + item.Value;
            }

            DesignExplorerData.Add("DataTitle", dataKey);
            DesignExplorerData.Add("DataValue", dataValue);
            DesignExplorerData.Add("FlyID", flyID);


            return DesignExplorerData;

        }


        public static int CalTotalCounts(List<ColibriParam> ColibriParams)
        {
            int totalIterations = 1;

            foreach (var item in ColibriParams)
            {
                //Cal total number of iterations
                int count = item.TotalCount;

                if (count > 0)
                {
                    totalIterations *= count;
                }

            }

            return totalIterations;
        }

        //to get all params' all steps' indexes 
        public static List<List<int>> AllParamsStepsIndex(List<ColibriParam> ColibriParams)
        {
            var stepsList = new List<List<int>>();
            foreach (var item in ColibriParams)
            {
                int totalCount = item.TotalCount;
                if (totalCount > 0)
                {
                    var SetpList = Enumerable.Range(0, totalCount).ToList();
                    stepsList.Add(SetpList);
                }
            }
            return stepsList;
            //inputParamsStepLists = stepLists;
        }
        

    }
    
}
