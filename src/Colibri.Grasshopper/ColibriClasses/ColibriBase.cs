using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colibri.Grasshopper
{
    public enum InputType { Slider, Panel, ValueList, Unsupported }
    public enum OverrideMode
    {
        OverrideAll,
        AppendAllToTheEnd,
        FinishTheRest,
        AskEverytime
    }
    static class ColibriBase
    {
        //public static Dictionary<string, string> ConvertToDictionary(Dictionary<string, string> Dictionary)
        //{
        //    return Dictionary;
        //}

        public static Dictionary<string, string> ConvertToDictionary (List<string> StringLikeDictionary)
        {
            var newDictionary = new Dictionary<string, string>();

            foreach (var item in StringLikeDictionary)
            {
                var cleanItem = item.ToString();
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
            var convertedDictionary = ConvertToDictionary(StringLikeDictionary);

            var DesignExplorerData = new Dictionary<string , string>();

            string dataKey = string.Empty;
            string dataValue = string.Empty;
            string flyID = string.Empty;
            
            foreach (var item in convertedDictionary)
            {
                dataKey = String.IsNullOrEmpty(dataKey) ? Prefix + item.Key : dataKey + "," + Prefix + item.Key;
				dataValue = String.IsNullOrEmpty(dataValue) ? item.Value : dataValue + "," + item.Value;
                flyID = String.IsNullOrEmpty(flyID) ? item.Key +  item.Value : flyID + "_" + item.Key + item.Value;
            }

            DesignExplorerData.Add("DataTitle", dataKey);
            DesignExplorerData.Add("DataValue", dataValue);
            DesignExplorerData.Add("FlyID", flyID);
            
            return DesignExplorerData;

        }


        public static Int64 CalTotalCounts(List<ColibriParam> ColibriParams)
        {
            Int64 totalIterations = 1;

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
        public static List<List<int>> AllParamsPositions(List<ColibriParam> ColibriParams)
        {
            var positionsList = new List<List<int>>();
            foreach (var item in ColibriParams)
            {
                int totalCount = item.TotalCount;
                if (totalCount > 0)
                {
                    var SetpList = Enumerable.Range(0, totalCount).ToList();
                    positionsList.Add(SetpList);
                }
            }
            return positionsList;
            //inputParamsStepLists = stepLists;
        }

        public static List<string> getAllNames(List<ColibriParam> ColibriParams)
        {
            var names = new List<string>();

            foreach (var item in ColibriParams)
            {
               names.Add(item.NickName);
            }

            return names;
        }


    }
    
}
