using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class ObjFileReader
    {

        public static Polygon ReadFile(string path)
        {

            char[] split = new char[] { ' ' };

            Polygon polygon = new Polygon();
            string mtlName = null;

            //  Create a stream reader.
            using (StreamReader reader = new StreamReader(path))
            {
                //  Read line by line.
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    //  Skip any comments (lines that start with '#').
                    if (line.StartsWith("#"))
                        continue;

                    //  Do we have a texture coordinate?
                    if (line.StartsWith("vt"))
                    {
                        //  Get the texture coord strings.
                        string[] values = line.Substring(3).Split(split, StringSplitOptions.RemoveEmptyEntries);

                        //  Parse texture coordinates.
                        float u = float.Parse(values[0]);
                        float v = float.Parse(values[1]);

                        //  Add the texture coordinate.
                        polygon.UVs.Add(new UV(u, v));
                        continue;
                    }

                    //  Do we have a normal coordinate?
                    if (line.StartsWith("vn"))
                    {
                        //  Get the normal coord strings.
                        string[] values = line.Substring(3).Split(split, StringSplitOptions.RemoveEmptyEntries);

                        //  Parse normal coordinates.
                        float x = float.Parse(values[0]);
                        float y = float.Parse(values[1]);
                        float z = float.Parse(values[2]);

                        //  Add the normal.
                        polygon.Normals.Add(new Vertex(x, y, z));
                        continue;
                    }

                    //  Do we have a vertex?
                    if (line.StartsWith("v"))
                    {
                        //  Get the vertex coord strings.
                        string[] values = line.Substring(2).Split(split, StringSplitOptions.RemoveEmptyEntries);

                        //  Parse vertex coordinates.
                        float x = float.Parse(values[0]);
                        float y = float.Parse(values[1]);
                        float z = float.Parse(values[2]);

                        //   Add the vertices.
                        polygon.Vertices.Add(new Vertex(x, y, z));

                        continue;
                    }

                    //  Do we have a face?
                    if (line.StartsWith("f"))
                    {
                        Face face = new Face();

                        //if (!String.IsNullOrWhiteSpace(mtlName))
                        //    face.Material = scene.Assets.Where(t => t.Name == mtlName).FirstOrDefault() as Material;

                        //  Get the face indices
                        string[] indices = line.Substring(2).Split(split,
                            StringSplitOptions.RemoveEmptyEntries);

                        //  Add each index.
                        foreach (var index in indices)
                        {
                            //  Split the parts.
                            string[] parts = index.Split(new char[] { '/' }, StringSplitOptions.None);

                            //  Add each part.
                            face.Indices.Add(new Index(
                                (parts.Length > 0 && parts[0].Length > 0) ? int.Parse(parts[0]) - 1 : -1,
                                (parts.Length > 1 && parts[1].Length > 0) ? int.Parse(parts[1]) - 1 : -1,
                                (parts.Length > 2 && parts[2].Length > 0) ? int.Parse(parts[2]) - 1 : -1));
                        }



                        //  Add the face.
                        polygon.Faces.Add(face);

                        continue;
                    }

                    //if (line.StartsWith("mtllib"))
                    //{
                    //    // Set current directory in case a relative path to material file is used.
                    //    Environment.CurrentDirectory = Path.GetDirectoryName(path);

                    //    // Load materials file.
                    //    string mtlPath = ReadMaterialValue(line);
                    //    LoadMaterials(mtlPath, scene);
                    //}

                    //if (line.StartsWith("usemtl"))
                    //    mtlName = ReadMaterialValue(line);
                }
            }

            return (polygon);
        }
    }
}
