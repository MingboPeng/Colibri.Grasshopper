using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Windows.Forms;
namespace Uploader
{
    public class Uploader : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        /// 
        public Uploader()
          : base("Uploader", "Uploader",
              "Uploader",
              "Colibri", "Colibri")
        {
        }

        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Colibri";

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("folderPath", "folder", "Folder path", GH_ParamAccess.item);
            pManager.AddBooleanParameter("upload", "upload", "Uploading data will take some time!", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("DesignExplorer Link", "DE_link", "Copy link to Chrome, happy explorering!", GH_ParamAccess.item);
        }

        private static string GetMimeType(string fileName)
        {
            string mimeType = "application/unknown";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey regKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (regKey != null && regKey.GetValue("Content Type") != null)
                mimeType = regKey.GetValue("Content Type").ToString();
            return mimeType;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //input variables
            string folder = "";
            List<string> pathLinks = new List<string>();
            //string filePath = @"C:\Users\Mingbo\Documents\GitHub\Colibri.Grasshopper\docs\GoogleDrive\test.png";
            bool writeFile = false;
            
            //get data
            DA.GetData(0, ref folder);
            DA.GetData(1, ref writeFile);

            //operations
            
            //GetMimeType


        bool run = writeFile;

            if (run)
            {
                UserCredential credential;

               // using (var stream =
                //    new FileStream(@"C:\ladybug\client_secret.json", FileMode.Open, FileAccess.Read))
               // {
                 //   string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                    //string credPath = @"C:\ladybug\";
                 //   credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
                    {
                        ClientId = "1053044124868-avf2ae7kqj6aeed3k5p3lg95tom8oafm.apps.googleusercontent.com",
                        ClientSecret = "p88yt1z0zohziAaSfEyOZk56"
                    },
                                                          new[] { DriveService.Scope.Drive,
                                                                  DriveService.Scope.DriveFile },
                                                          "user",
                                                          CancellationToken.None,
                                                          new FileDataStore("Drive.Auth.Store")).Result;

                  //  credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                   //     GoogleClientSecrets.Load(stream).Secrets,
                   //     Scopes,
                   //     "user",
                   //     CancellationToken.None,
                   //     new FileDataStore(credPath, true)).Result;
                   // Console.WriteLine("Credential file saved to: " + credPath);
               // }

                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });


                

                //FilesResource.ListRequest listRequest = service.Files.List();
                var fileMetadata = new Google.Apis.Drive.v3.Data.File();
                
                fileMetadata.Name = "DesignExplorer_Colibri";
                fileMetadata.MimeType = "application/vnd.google-apps.folder";
                //fileMetadata.Parents = new List<string> { "0B8secD5h7wUFVW5GYmtZR3hqZkk" }; // play later
                FilesResource.CreateRequest createRequest = service.Files.Create(fileMetadata);
                ////var request = driveService.Files.Create(fileMetadata);
                 createRequest.Fields = "id";
                 var file = createRequest.Execute();
                //Console.WriteLine("Folder ID: " + file.Id);
                //Permission permission = new Permission ();
                //permission.Role = "owner";
                //permission.Type = "anyone";
                //service.Permissions.Create(permission, file.Id).Execute();



                string[] fileEntries = Directory.GetFiles(folder);
                
                foreach (string fileName in fileEntries) {
                    //dosomething;
                    Google.Apis.Drive.v3.Data.File body = new Google.Apis.Drive.v3.Data.File();
                    body.Name = System.IO.Path.GetFileName(fileName);
                    body.Description = "File uploaded by Colibri!";
                    body.MimeType = GetMimeType(fileName);
                    body.Parents = new List<string> { file.Id };

                    // File's content.
                    byte[] byteArray = System.IO.File.ReadAllBytes(fileName);
                    System.IO.MemoryStream stream2 = new System.IO.MemoryStream(byteArray);

                    try
                    {
                        FilesResource.CreateMediaUpload request = service.Files.Create(body, stream2, GetMimeType(fileName));
                        request.Upload();
                        var fileitem = request.ResponseBody;
                    }
                    catch (Exception e)
                    {
                        //A =("An error occurred: " + e.Message);
                    }
                    //file.Id;

                }
                MessageBox.Show("http://tt-acm.github.io/DesignExplorer/?GFOLDER=" + file.Id, "test1", MessageBoxButtons.OK);

                DA.SetData(0, "http://tt-acm.github.io/DesignExplorer/?GFOLDER="+ file.Id);
            }
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
                return Colibri.Grasshopper.Properties.Resources.Colibri_logobase_5;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{787196c8-5cc8-46f5-b253-4e63d8d271e3}"); }
        }
    }
}
