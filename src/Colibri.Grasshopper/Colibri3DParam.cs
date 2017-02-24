using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using System.Dynamic;
using System.Linq;

using Newtonsoft.Json;

namespace Colibri.Grasshopper
{
    public class Colibri3DParam : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Colibri3DParam class.
        /// </summary>
        public Colibri3DParam()
          : base("3D Parameters", "3D Param",
              "Defines how Colibri generates 3D models.",
              "TT Toolbox", "Colibri")
        {
        }

        public override GH_Exposure Exposure { get { return GH_Exposure.secondary; } }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Lines", "Lines", "Lines or Curves that will be exported", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;

            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes that will be exported with its current material if it exists", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[1].DataMapping = GH_DataMapping.Flatten;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("3d Element", "3DParams", "3d element output to feed into Aggregator component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var lines = new List<GH_Line>();
            var meshes = new List<GH_Mesh>();
            DA.GetDataList(0, lines);
            DA.GetDataList(1, meshes);

            if (meshes.Any() || lines.Any())
            {
                //create json from mesh
                string outJSON = makeJsonString(meshes,lines);
                outJSON = outJSON.Replace("OOO", "object");

                DA.SetData(0, outJSON);
            }
            
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{711f50aa-b437-4850-af56-ac2e7db19568}"); }
        }

        
        public static string hexColor(GH_Colour ghColor)
        {
            string hexStr = "0x" + ghColor.Value.R.ToString("X2") +
                ghColor.Value.G.ToString("X2") +
                ghColor.Value.B.ToString("X2");

            return hexStr;
        }

        #region getLineForJson
        private dynamic lineJSON(Line line)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            //top level properties
            jason.uuid = Guid.NewGuid();
            jason.type = "Geometry";
            jason.data = new ExpandoObject();

            //populate data object properties
            jason.data.vertices = new object[6];
            jason.data.vertices[0] = Math.Round(line.FromX * -1.0, 5);
            jason.data.vertices[1] = Math.Round(line.FromZ, 5);
            jason.data.vertices[2] = Math.Round(line.FromY, 5);
            jason.data.vertices[3] = Math.Round(line.ToX * -1.0, 5);
            jason.data.vertices[4] = Math.Round(line.ToZ, 5);
            jason.data.vertices[5] = Math.Round(line.ToY, 5);
            jason.data.normals = new object[0];
            //jason.data.uvs = new object[0];
            jason.data.faces = new object[0];
            jason.data.scale = 1;
            jason.data.visible = true;
            //jason.data.castShadow = true;
            //jason.data.receiveShadow = false;


            //return
            return jason;
            //return JsonConvert.SerializeObject(jason);
        }

        private object makeLineMaterial()
        {
            dynamic JsonMat = new ExpandoObject();
            JsonMat.uuid = Guid.NewGuid();
            JsonMat.type = "LineBasicMaterial";
            JsonMat.color = "0x000000";
            JsonMat.linewidth = 1;
            JsonMat.opacity = 1;

            return JsonMat;
        }

        private dynamic linesJSON (List<GH_Line> GHLines)
        {

            if (!GHLines.Any())
            {
                return null;
            }
            
            var JsonGeometries = new List<object>();
            foreach (var item in GHLines)
            {
                JsonGeometries.Add(lineJSON(item.Value));
            }

            int size = JsonGeometries.Count;


            dynamic JsonFile = new ExpandoObject();
            JsonFile.geometries = JsonGeometries;
            JsonFile.materials = makeLineMaterial();
            JsonFile.children = new object[size];
            //dynamic userData = new ExpandoObject();
            //userData.layer = "Default";


            int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            for (int i = 0; i < size; i++)
            {
                dynamic JsonObjectChildren = new ExpandoObject();
                JsonObjectChildren.uuid = Guid.NewGuid();
                JsonObjectChildren.name = "Line " + i.ToString();
                JsonObjectChildren.type = "Line";
                JsonObjectChildren.geometry = JsonFile.geometries[i].uuid;
                JsonObjectChildren.material = JsonFile.materials.uuid;
                //JsonObjectChildren.matrix = numbers;
                //JsonObjectChildren.userData = userData;

                //add children to JsonFile
                JsonFile.children[i] = JsonObjectChildren;
            }
            
            
            return JsonFile;
        }

        #endregion

        #region getMeshForJson
        /// <summary>
        /// Returns a JSON string representing a rhino mesh, and containing any attributes as user data
        /// </summary>
        /// <param name="mesh">The rhino mesh to serialize.  Can contain quads and tris.</param>
        /// <param name="attDict">The attribute dictionary to serialize.  Objects should all be reference types.</param>
        /// <returns>a JSON string representing a rhino mes</returns>
        private static object meshJSON(Mesh mesh)
        {
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();


            jason.uuid = Guid.NewGuid();
            jason.type = "Geometry";
            jason.data = new ExpandoObject();

            //populate data object properties

            //fisrt, figure out how many faces we need based on the tri/quad count

            var quads = from q in mesh.Faces
                        where q.IsQuad
                        select q;

            jason.data.vertices = new object[mesh.Vertices.Count * 3];
            jason.data.faces = new object[(mesh.Faces.Count + quads.Count()) * 4];
            jason.data.scale = 1;
            jason.data.visible = true;
            jason.data.castShadow = true;
            jason.data.receiveShadow = false;
            jason.data.doubleSided = true;

            //populate vertices
            int counter = 0;
            int i = 0;
            foreach (var v in mesh.Vertices)
            {
                jason.data.vertices[counter++] = Math.Round(mesh.Vertices[i].X * -1.0, 5);
                jason.data.vertices[counter++] = Math.Round(mesh.Vertices[i].Z, 5);
                jason.data.vertices[counter++] = Math.Round(mesh.Vertices[i].Y, 5);
                i++;
            }

            //populate faces
            counter = 0;
            i = 0;
            foreach (var f in mesh.Faces)
            {
                if (f.IsTriangle)
                {
                    jason.data.faces[counter++] = 0;
                    jason.data.faces[counter++] = mesh.Faces[i].A;
                    jason.data.faces[counter++] = mesh.Faces[i].B;
                    jason.data.faces[counter++] = mesh.Faces[i].C;
                    i++;
                }
                if (f.IsQuad)
                {
                    jason.data.faces[counter++] = 0;
                    jason.data.faces[counter++] = mesh.Faces[i].A;
                    jason.data.faces[counter++] = mesh.Faces[i].B;
                    jason.data.faces[counter++] = mesh.Faces[i].C;
                    jason.data.faces[counter++] = 0;
                    jason.data.faces[counter++] = mesh.Faces[i].C;
                    jason.data.faces[counter++] = mesh.Faces[i].D;
                    jason.data.faces[counter++] = mesh.Faces[i].A;
                    i++;
                }
            }

            //populate vertex colors
            //if (mesh.VertexColors.Count != 0)
            //{
            //    jason.data.colors = new object[mesh.Vertices.Count];
            //    i = 0;
            //    foreach (var c in mesh.VertexColors)
            //    {
            //        jason.data.colors[i] = hexColor(new GH_Colour(c));
            //        i++;
            //    }
            //}

            return jason;
            //return JsonConvert.SerializeObject(jason);
        }

        private Dictionary<string, object> getMeshFaceMaterials(Mesh mesh)
        {
            var JSON = new Dictionary<string, object>();
            
            dynamic JsonMat = new ExpandoObject();
            JsonMat.uuid = Guid.NewGuid();
            JsonMat.type = "MeshFaceMaterial";

            //we need an list of material indexes, one for each face of the mesh.  This will be stroed as a CSV string in the attributes dict
            //and on the viewer side we'll use this to set each mesh face's material index property
            List<int> myMaterialIndexes = new List<int>();

            //since some faces might share a material, we'll keep a local dict of materials to avoid duplicates
            //key = hex color, value = int representing a material index
            Dictionary<string, int> faceMaterials = new Dictionary<string, int>();


            int matCounter = 0;
            int uniqueColorCounter = 0;

            var meshColors = mesh.VertexColors.ToList();


            foreach (var face in mesh.Faces)
            {
                //make sure there is an item at this index.  if not, grab the last one
                if (matCounter == mesh.Faces.Count)
                {
                    matCounter = mesh.Faces.Count = 1;
                }
                //if (matCounter > colors.Count - 1) matCounter = colors.Count - 1;

                //get a string representation of the color
                int firstFaceVertexIndex = mesh.Faces.GetFace(matCounter).A;
                //default color
                string myColorStr = "0x677A85";
                //change to mesh face color
                if (meshColors.Any())
                {
                    myColorStr = hexColor(new GH_Colour(meshColors[firstFaceVertexIndex]));
                }


                //check to see if we need to create a new material index
                if (!faceMaterials.ContainsKey(myColorStr))
                {
                    //add the color/index pair to our dictionary and increment the unique color counter
                    faceMaterials.Add(myColorStr, uniqueColorCounter);
                    uniqueColorCounter++;
                }

                //add the color[s] to the array.  one for a tri, two for a quad
                if (face.IsTriangle)
                {
                    myMaterialIndexes.Add(faceMaterials[myColorStr]);
                }
                else if (face.IsQuad)
                {
                    myMaterialIndexes.Add(faceMaterials[myColorStr]);
                    myMaterialIndexes.Add(faceMaterials[myColorStr]);
                }
                matCounter++;
            }

            JsonMat.materials = new object[faceMaterials.Count];
            for (int i = 0; i < faceMaterials.Count; i++)
            {
                dynamic matthew = new ExpandoObject();
                matthew.uuid = Guid.NewGuid();
                matthew.type = "MeshBasicMaterial";
                matthew.side = 2;
                matthew.color = faceMaterials.Keys.ToList()[i];
                JsonMat.materials[i] = matthew;
            }

            JSON.Add("materials", JsonMat);
            JSON.Add("faceMaterialIndex", String.Join(",", myMaterialIndexes));

            return JSON;
            //return JsonConvert.SerializeObject(JsonMat);
        }

        private dynamic meshesJSON(List<GH_Mesh> GHMeshes)
        {

            if (!GHMeshes.Any())
            {
                return null;
            }

            Mesh joinedMesh = new Mesh();
            foreach (var item in GHMeshes)
            {
                joinedMesh.Append(item.Value);
            }

            var JsonGeometries = new List<object> { meshJSON(joinedMesh) };
            var materialsInfo = getMeshFaceMaterials(joinedMesh);

            int size = JsonGeometries.Count;

            dynamic JsonFile = new ExpandoObject();
            JsonFile.geometries = JsonGeometries;
            JsonFile.materials = materialsInfo["materials"];
            JsonFile.children = new object[size];

            dynamic userData = new ExpandoObject();
            userData.Spectacles_FaceColorIndexes = materialsInfo["faceMaterialIndex"];
            
            //int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            for (int i = 0; i < size; i++)
            {
                dynamic JsonObjectChildren = new ExpandoObject();
                JsonObjectChildren.uuid = Guid.NewGuid();
                JsonObjectChildren.name = "mesh" + i.ToString();
                JsonObjectChildren.type = "Mesh";
                JsonObjectChildren.geometry = JsonFile.geometries[i].uuid;
                JsonObjectChildren.material = JsonFile.materials.uuid;
                //JsonObjectChildren.matrix = numbers;
                JsonObjectChildren.userData = userData;

                //add children to JsonFile
                JsonFile.children[i] = JsonObjectChildren;
            }
            
            return JsonFile;
        }

        #endregion


        private dynamic makeJsonString(List<GH_Mesh> meshes, List<GH_Line> lines)
        {
            var meshesJsonObj = meshesJSON(meshes);
            var linesJsonObj = linesJSON(lines);

            var JsonGeometries = new List<object>();
            var JsonMaterials = new List<object>();
            var JsonChildren = new List<object>();

            if (meshesJsonObj!=null)
            {
                JsonGeometries.AddRange(meshesJsonObj.geometries);
                JsonMaterials.Add(meshesJsonObj.materials);
                JsonChildren.AddRange(meshesJsonObj.children);
            }

            if (linesJsonObj!=null)
            {
                JsonGeometries.AddRange(linesJsonObj.geometries);
                JsonMaterials.Add(linesJsonObj.materials);
                JsonChildren.AddRange(linesJsonObj.children);
            }
            



            //dynamic meshObj = meshesJSON(meshes);
            //var meshMaterial = getMeshFaceMaterials(meshes);
            
            //var faceMaterialIndex = meshMaterial.faceMaterialIndex;

            //create a dynamic object to populate
            dynamic outJsonFile = new ExpandoObject();
            
            int geoSize = JsonGeometries.Count;
            int metSize = JsonMaterials.Count;

            //create geometries placeholders:
            //outJsonFile.geometries = new object[geoSize];
            outJsonFile.geometries = JsonGeometries;
            //create materials placeholders:
            //outJsonFile.materials = new object[metSize];
            outJsonFile.materials = JsonMaterials;

            //create scene placeholder:
            outJsonFile.OOO = new ExpandoObject();
            outJsonFile.OOO.uuid = System.Guid.NewGuid();
            outJsonFile.OOO.type = "Scene";
            //int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            outJsonFile.OOO.matrix = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            outJsonFile.OOO.children = JsonChildren;




            //for (int i = 0; i < geoSize; i++)
            //{
            //    outJsonFile.geometries[i] = JsonGeometries[i];
            //    outJsonFile.OOO.children[i] = JsonChildren[i];
            //}
            
            
            //return jason;
            return JsonConvert.SerializeObject(outJsonFile);
        }

    }


}
