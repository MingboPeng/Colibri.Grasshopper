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
          : base("Colibri3DParam", "3D Param",
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
            pManager.AddCurveParameter("Lines", "Lines", "Lines or Curves", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;

            pManager.AddMeshParameter("Meshes", "Meshes", "Meshes", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[1].DataMapping = GH_DataMapping.Flatten;

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("3d Element", "threeD", "3d element output to feed into Aggregator component", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curves = new List<GH_Curve>();
            var meshes = new List<GH_Mesh>();
            DA.GetDataList(0, curves);
            DA.GetDataList(1, meshes);

            if (meshes==null)
            {
                return;
            }

            Mesh joinedMesh = new Mesh();
            foreach (var item in meshes)
            {
                joinedMesh.Append(item.Value);
            }
            
            //create json from mesh
            var outJSON = makeJsonString(joinedMesh);
            outJSON = outJSON.Replace("OOO", "object");
            //Material material = new Material(MaterialWithVertexColors(), SpectaclesMaterialType.Mesh);
            //Element e = new Element(outJSON, SpectaclesElementType.Mesh, material, new Layer("Default"));

            DA.SetData(0, outJSON);

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

        /// <summary>
        /// Returns a JSON string representing a rhino mesh, and containing any attributes as user data
        /// </summary>
        /// <param name="mesh">The rhino mesh to serialize.  Can contain quads and tris.</param>
        /// <param name="attDict">The attribute dictionary to serialize.  Objects should all be reference types.</param>
        /// <returns>a JSON string representing a rhino mes</returns>
        public static dynamic geoJSON(Mesh mesh)
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

        public dynamic getMeshFaceMaterials(Mesh mesh)
        {
            dynamic JSON = new ExpandoObject();
            JSON.materials = new ExpandoObject();
            JSON.

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
                int firstFaceVertexIndex = mesh.Faces.GetTopologicalVertices(matCounter).First();
                string myColorStr = hexColor(new GH_Colour(meshColors[firstFaceVertexIndex]));

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
                if (face.IsQuad)
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

            return JsonMat;
            //return JsonConvert.SerializeObject(JsonMat);
        }

        private string makeJsonString(Mesh mesh)
        {
            var meshObj = geoJSON(mesh);
            var meshMaterial = getMeshFaceMaterials(mesh);

            
            //create a dynamic object to populate
            dynamic jason = new ExpandoObject();

            ////JSON.metadata metadata object
            //jason.metadata = new ExpandoObject();
            //jason.metadata.version = 4.3;
            //jason.metadata.type = "Object";
            //jason.metadata.generator = "Spectacles_Grasshopper_Exporter";

            int size = 1;

            //populate mesh geometries:
            jason.geometries = new object[size];   //array for geometry - both lines and meshes
            jason.materials = new object[size];  //array for materials - both lines and meshes

            jason.geometries[0] = meshObj;
            jason.materials[0] = meshMaterial;

            //create scene:
            jason.OOO = new ExpandoObject();
            jason.OOO.uuid = System.Guid.NewGuid();
            jason.OOO.type = "Scene";
            int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            jason.OOO.matrix = numbers;
            jason.OOO.children = new object[size];

            
            //create childern
            //loop over meshes and lines
            int i = 0;
            //foreach (var meshItem in meshObj.) //meshes
            //{
                jason.OOO.children[i] = new ExpandoObject();
                jason.OOO.children[i].uuid = Guid.NewGuid();
                jason.OOO.children[i].name = "mesh" + i.ToString();
                jason.OOO.children[i].type = "Mesh";
                jason.OOO.children[i].geometry = meshObj.uuid;
                jason.OOO.children[i].material = meshMaterial.uuid;
                jason.OOO.children[i].matrix = numbers;
                jason.OOO.children[i].userData = "userdata!!";
                //i++;
            //}

            return JsonConvert.SerializeObject(jason);
        }

    }


}
