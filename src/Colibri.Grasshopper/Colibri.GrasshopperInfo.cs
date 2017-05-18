using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Colibri.Grasshopper
{
    public class ColibriGrasshopperInfo : GH_AssemblyInfo
  {
    public override string Name
    {
        get
        {
            return "Colibri";
        }
    }

        public override string AssemblyVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public override GH_LibraryLicense AssemblyLicense
        {
            get
            {
                return GH_LibraryLicense.opensource;
            }            
        }

        public override Bitmap Icon
    {
        get
        {
            //Return a 24x24 pixel bitmap to represent this GHA library.
            return Properties.Resources.Aggregator;
        }
    }
    public override string Description
    {
        get
        {
            //Return a short string describing the purpose of this GHA library.
            return "Exports design spaces from Grasshopper to Design Explorer";
        }
    }
    public override Guid Id
    {
        get
        {
            return new Guid("b96948a9-4599-447d-b3bc-18f88e319755");
        }
    }

    public override string AuthorName
    {
        get
        {
            //Return a string identifying you or your company.
            return "CORE studio | Thornton Tomasetti";
        }
    }
    public override string AuthorContact
    {
        get
        {
            //Return a string representing your preferred contact details.
            return "ttcorestudio@gmail.com";
        }
    }
}
}
