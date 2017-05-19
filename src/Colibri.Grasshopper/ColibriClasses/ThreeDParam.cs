using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using System.Dynamic;

namespace Colibri.Grasshopper
{
    public class threeDParam
    {
        public bool IsDefined { get; set; }

        public string JsonSting { get; set; }
        //private List<object> JsonGeometries { get; set; }
        //private List<object> JsonMaterials { get; set; }
        //private List<object> JsonChildren { get; set; }
        
        public threeDParam()
        {
            this.IsDefined = false;
        }
        
        public threeDParam(List<object> threeDObjs)
        {
            if (threeDObjs.Any())
            {
               
                object outJSON = makeThreeDParamJSON(threeDObjs);

                if (outJSON != null)
                {
                    this.IsDefined = true;
                    this.JsonSting = JsonConvert.SerializeObject(outJSON);
                    this.JsonSting = JsonSting.Replace("OOO", "object");
                }
                
            }
            
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
            JsonMat.uuid = "LnColor_Black";
            JsonMat.type = "LineBasicMaterial";
            JsonMat.color = "0x000000";
            JsonMat.linewidth = 1;
            JsonMat.opacity = 1;

            return JsonMat;
        }

        private dynamic linesJSON(List<GH_Line> GHLines)
        {

            if (GHLines.IsNullOrEmpty())
            {
                return null;
            }

            //LineCount = GHLines.Count;
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


            //int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
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
        private static object meshGeometries(Mesh mesh)
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

            //mesh is uncolored
            if (meshColors.IsNullOrEmpty())
            {
                JsonMat.materials = new object[1];
                dynamic matthew = new ExpandoObject();
                matthew.uuid = Guid.NewGuid();
                matthew.type = "MeshLambertMaterial";
                matthew.color = "0xecf0f1";
                matthew.ambient = "0xecf0f1";
                matthew.emissive = 0;
                matthew.opacity = 1;
                matthew.transparent = false;
                matthew.wireframe = false;
                matthew.shading = 1;
                JsonMat.materials[0] = matthew;

                JSON.Add("materials", JsonMat);
                JSON.Add("faceMaterialIndex", "default");
                return JSON;

            }
            else
            {
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
                    string myColorStr = "0xecf0f1";
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
                JSON.Add("faceMaterialIndex", string.Join(",", myMaterialIndexes));

                return JSON;
            }
            
        }

        private dynamic meshesJSON(List<GH_Mesh> GHMeshes)
        {

            if (GHMeshes.IsNullOrEmpty())
            {
                return null;
            }

            //MeshCount = GHMeshes.Count;
            var JsonGeometries = new List<object>();
            var JsonMaterials = new List<object>();
            var JsonFaceColorIndexes = new List<object>();
            //Mesh joinedMesh = new Mesh();
            foreach (var item in GHMeshes)
            {
                //joinedMesh.Append(item.Value);
                var meshMaterials = getMeshFaceMaterials(item.Value);
                JsonGeometries.Add(meshGeometries(item.Value));
                JsonMaterials.Add(meshMaterials["materials"]);
                JsonFaceColorIndexes.Add(meshMaterials["faceMaterialIndex"]);
                
            }

            //var JsonGeometries = new List<object> { meshGeometries(joinedMesh) };
            //var materialsInfo = getMeshFaceMaterials(joinedMesh);

            int size = JsonGeometries.Count;

            dynamic JsonFile = new ExpandoObject();
            JsonFile.geometries = JsonGeometries;
            JsonFile.materials = JsonMaterials;
            JsonFile.children = new object[size];
            
            //int[] numbers = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            for (int i = 0; i < size; i++)
            {
                dynamic JsonObjectChildren = new ExpandoObject();
                JsonObjectChildren.uuid = Guid.NewGuid();
                JsonObjectChildren.name = "mesh" + i.ToString();
                JsonObjectChildren.type = "Mesh";
                JsonObjectChildren.geometry = JsonFile.geometries[i].uuid;
                JsonObjectChildren.material = JsonFile.materials[i].uuid;
                //JsonObjectChildren.matrix = numbers;

                //userData
                dynamic userData = new ExpandoObject();
                if (JsonFaceColorIndexes[i].ToString() != "default")
                {
                    userData.Spectacles_FaceColorIndexes = JsonFaceColorIndexes[i];
                }
                
                JsonObjectChildren.userData = userData;

                //add children to JsonFile
                JsonFile.children[i] = JsonObjectChildren;
            }

            return JsonFile;
        }


        #endregion

        #region getSpectaclesForJson
        private dynamic spectaclesJSON(List<dynamic> SpectaclesObjs)
        {
            if (SpectaclesObjs.IsNullOrEmpty()) return null;

            var JsonGeometries = new List<object>();
            var JsonMaterials = new List<object>();
            var JsonChildren = new List<object>();


            foreach (var item in SpectaclesObjs)
            {
                dynamic obj = item;

                JsonGeometries.AddRange(obj.geometries);
                JsonMaterials.AddRange(obj.materials);
                JsonChildren.AddRange(obj.OOO.children);
            }

            dynamic JsonFile = new ExpandoObject();
            JsonFile.geometries = JsonGeometries;
            JsonFile.materials = JsonMaterials;
            JsonFile.children = JsonChildren;

            return JsonFile;

        }
        #endregion


        //int LineCount = 0;
        //int MeshCount = 0;

        private object makeThreeDParamJSON(List<object> GHGeometries)
        {
            var validObjs = collectValidObjs(GHGeometries);

            var lines = validObjs["lines"] as List<GH_Line>;
            var meshes = validObjs["meshes"] as List<GH_Mesh>;
            var spectaclesObjs = validObjs["SpectaclesObjs"] as List<dynamic>;

           
            var meshesJsonObj = meshesJSON(meshes);
            var linesJsonObj = linesJSON(lines);
            var spectaclesJsonObj = spectaclesJSON(spectaclesObjs);

            var JsonGeometries = new List<object>();
            var JsonMaterials = new List<object>();
            var JsonChildren = new List<object>();

            if (meshesJsonObj != null)
            {
                JsonGeometries.AddRange(meshesJsonObj.geometries);
                JsonMaterials.AddRange(meshesJsonObj.materials);
                JsonChildren.AddRange(meshesJsonObj.children);
            }

            if (linesJsonObj != null)
            {
                JsonGeometries.AddRange(linesJsonObj.geometries);
                JsonMaterials.Add(linesJsonObj.materials);
                JsonChildren.AddRange(linesJsonObj.children);
            }

            if (spectaclesJsonObj != null)
            {
                JsonGeometries.AddRange(spectaclesJsonObj.geometries);
                JsonMaterials.AddRange(spectaclesJsonObj.materials);
                JsonChildren.AddRange(spectaclesJsonObj.children);
            }

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
            outJsonFile.OOO.matrix = new int[16] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
            outJsonFile.OOO.children = JsonChildren;

            return outJsonFile;
        }

        private Dictionary<string, object> collectValidObjs(List<object> inputObjects)
        {
            var lines = new List<GH_Line>();
            var meshes = new List<GH_Mesh>();
            var SpectaclesObjs = new List<dynamic>();
            foreach (var item in inputObjects)
            {

                if (item is GH_Line)
                {
                    lines.Add(item as GH_Line);

                }
                else if (item is GH_Curve)
                {
                    var i = item as GH_Curve;
                    var pol = new Polyline();
                    i.Value.TryGetPolyline(out pol);
                    var segments = pol.GetSegments();
                    foreach (var line in segments)
                    {
                        lines.Add(new GH_Line(line));
                    }
                    
                }
                else if (item is GH_Mesh)
                {
                    meshes.Add(item as GH_Mesh);

                }
                else if (item is GH_Brep)
                {
                    var i = item as GH_Brep;
                    var mesh = brepToGHMesh(i.Value);
                    meshes.Add(mesh);

                }
                else if (item is GH_Box)
                {
                    var i = item as GH_Box;
                    var mesh = brepToGHMesh(i.Brep());
                    meshes.Add(mesh);

                }
                else if (item is GH_Surface)
                {
                    var i = item as GH_Surface;
                    var mesh = brepToGHMesh(i.Value);
                    meshes.Add(mesh);

                }
                else if (item is GH_ObjectWrapper)
                {
                    var inSpec = item as GH_ObjectWrapper;
                    if (inSpec.Value is ExpandoObject)
                    {
                        SpectaclesObjs.Add(inSpec.Value);
                    }
                }
                else
                {

                }

            }

            var dic = new Dictionary<string, object>();
            //add lines
            dic.Add("lines", lines);
            dic.Add("meshes", meshes);
            dic.Add("SpectaclesObjs", SpectaclesObjs);
            
            return dic;
        }
        //private List<GH_Mesh> collectMeshes(List<object> inputObjects)
        //{
        //    var meshes = new List<GH_Mesh>();
        //    foreach (var item in inputObjects)
        //    {

        //        if (item is GH_Mesh)
        //        {
        //            meshes.Add(item as GH_Mesh);

        //        }
        //        else if (item is GH_Brep)
        //        {
        //            var i = item as GH_Brep;
        //            var mesh = brepToGHMesh(i.Value);
        //            meshes.Add(mesh);

        //        }
        //        else if (item is GH_Box)
        //        {
        //            var i = item as GH_Box;
        //            var mesh = brepToGHMesh(i.Brep());
        //            meshes.Add(mesh);

        //        }
        //        else if (item is GH_Surface)
        //        {
        //            var i = item as GH_Surface;
        //            var mesh = brepToGHMesh(i.Value);
        //            meshes.Add(mesh);

        //        }
        //        else
        //        {

        //        }

        //    }

            
        //    return meshes;
        //}
        


        private GH_Mesh brepToGHMesh(Brep brep)
        {
            Mesh m = new Mesh();
            var meshes = Mesh.CreateFromBrep(brep);
            foreach (var item in meshes)
            {
                m.Append(item);
            }

            return new GH_Mesh(m);

        }

        public static string hexColor(GH_Colour ghColor)
        {
            string hexStr = "0x" + ghColor.Value.R.ToString("X2") +
                ghColor.Value.G.ToString("X2") +
                ghColor.Value.B.ToString("X2");
            
            return hexStr;
        }

        public override string ToString()
        {
            string outputString = "3D Model for Design Explorer";
            return outputString;
        }
    }
}
