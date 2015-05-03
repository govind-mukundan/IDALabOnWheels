using Assimp;
using Assimp.Configs;
using GlmNet;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class ObjModelLoader
    {

        // To locate the start and end of a mesh vertex set in the VBO for the entire model
        class MeshVertexPointers{
           public int Start;
           public int End;
        }

        VertexBufferArray objVAO;

        Dictionary<string, Texture> TextureList = new Dictionary<string, Texture>();

        TextureManager _texManager;
        int _numMeshes;
        Assimp.Scene model;
        float MinX = 100000f;
        float MaxX = 0;
        float MinY = 100000f;
        float MaxY = 0;
        float MinZ = 100000f;
        float MaxZ = 0;

        public vec3 Centroid { get { return new vec3((MinX + MaxX) / 2, (MinY + MaxY) / 2, (MinZ + MaxZ)/2); } }
        MeshVertexPointers[] _meshPointer; //

        // Use a single color mapped into a texture for objects that don't come with a texture map
        string C_DUMMY_TEXTURE = "Dummy1x1"; // A dummy texture path for 1x1 textures that are created from colors


        public ObjModelLoader()
        {
            _texManager = TextureManager.Instance;//new TextureManager();
        }

        public void LoadObj(string path, OpenGL GL)
        {

            #region ASSIMP MOdel Loading

            // https://code.google.com/p/assimp-net/wiki/GettingStarted
            //Create a new importer
            AssimpContext importer = new AssimpContext();

            //This is how we add a configuration (each config is its own class)
            NormalSmoothingAngleConfig config = new NormalSmoothingAngleConfig(66.0f);
            importer.SetConfig(config);

            //This is how we add a logging callback 
            LogStream logstream = new LogStream(delegate(String msg, String userData)
            {
                Console.WriteLine(msg);
            });
            logstream.Attach();

            //Import the model. All configs are set. The model
            //is imported, loaded into managed memory. Then the unmanaged memory is released, and everything is reset.
            model = importer.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality);
            _numMeshes = model.MeshCount;

            //TODO: Load the model data into your own structures
            // http://www.mbsoftworks.sk/index.php?page=tutorials&series=1&tutorial=23
            // find the total number of faces in the object * 3 for number of vertices
            // Assume that a face is always a triangle. OBJ files may contain Quad faces, but ASSIMP will convert them into triangles to make our life easier
            int faces = 0;
            for (int j = 0; j < model.MeshCount; j++) // for each mesh
            {
                faces += model.Meshes[j].FaceCount; // find the total number of faces in the entire figure (= sum of faces for each mesh)
            }

            vec3[] vObjVertices = new vec3[faces * 3];
            vec3[] vObjNormals = new vec3[faces * 3];
            vec2[] vObjTextures = new vec2[faces * 3];
            vec3[] vColor = new vec3[faces * 3];
            _meshPointer = new MeshVertexPointers[model.MeshCount];
            // go through all the faces of the poly and get the vertices
            int i = 0; // points to the current vertex number [0 ... total vertices in figure]
            for (int j = 0; j < model.MeshCount; j++) // for each mesh
            {
                _meshPointer[j] = new MeshVertexPointers();
                _meshPointer[j].Start = i;
                for (int k = 0; k < model.Meshes[j].FaceCount; k++) // for each face in a mesh
                {
                    for (int q = 0; q < model.Meshes[j].Faces[k].IndexCount; q++,i++) // for each index in a face
                    {
                        Vector3D Vtemp = model.Meshes[j].HasVertices ? model.Meshes[j].Vertices[model.Meshes[j].Faces[k].Indices[q]] : new Vector3D(); // Find the vertex for that index
                        vObjVertices[i] = new vec3(Vtemp.X, Vtemp.Y, Vtemp.Z);
                        FindMaxMin(Vtemp);

                       Vector3D Ttemp = model.Meshes[j].HasTextureCoords(0) ? model.Meshes[j].TextureCoordinateChannels[0][model.Meshes[j].Faces[k].Indices[q]] : new Vector3D(); // Find the texture coordinates
                       vObjTextures[i] = new vec2(Ttemp.X, Ttemp.Y);

                       Vector3D Ntemp = model.Meshes[j].HasNormals ? model.Meshes[j].Normals[model.Meshes[j].Faces[k].Indices[q]] : new Vector3D(); // Find the normals
                       vObjNormals[i] = new vec3(Ntemp.X, Ntemp.Y, Ntemp.Z);

                       vColor[i] = GL.Color(Color.Red);
                    }

                }
                _meshPointer[j].End = i - 1;
            }

            // Now we have all the vertices and the attributes of each vertex. Next thing is to load the textures themselves
            // Usually textures are specified in a separate file (jpg/tga...) and the OBJ file just points to the name of this file for each mesh
            // So given a mesh, you can find it's "material" (texture file)

            // Load the materials/textures used for the OBJ
            for (int j = 0; j < model.MaterialCount; j++) // for each material in the OBJ
            {
                TextureSlot tSlot = new TextureSlot();
                if (model.Materials[j].GetMaterialTexture(TextureType.Diffuse, 0, out tSlot))
                {
                    _texManager.CreateTexture(AppDomain.CurrentDomain.BaseDirectory + "mesh\\texture\\" + tSlot.FilePath, GL);
                }
            }

            if (!model.HasTextures) // IF the model does not have any textures, lets create a texture from a color and use that
                _texManager.CreateTexture1x1(C_DUMMY_TEXTURE, GL, false, Color.Olive);

            // Materials and vertex properties are done, lets create the VAO

            objVAO = new VertexBufferArray();
            objVAO.Create(GL);
            objVAO.Bind(GL);

            VertexBuffer[] objVBO = new VertexBuffer[VertexAttributes.Instance.C_NUM_ATTRIB];

            objVBO[0] = new VertexBuffer();
            objVBO[0].Create(GL);
            objVBO[0].Bind(GL);
            objVBO[0].SetData(GL, VertexAttributes.Instance.AttrbPosition, vObjVertices, false, 3);

            //  Texture
            objVBO[1] = new VertexBuffer();
            objVBO[1].Create(GL);
            objVBO[1].Bind(GL);
            objVBO[1].SetData(GL, VertexAttributes.Instance.AttrbTexture, vObjTextures, false, 2);

            //  Normals
            objVBO[2] = new VertexBuffer();
            objVBO[2].Create(GL);
            objVBO[2].Bind(GL);
            objVBO[2].SetData(GL, VertexAttributes.Instance.AttrbSurfaceNormal, vObjNormals, false, 3);

            //objVBO[3] = new VertexBuffer();
            //objVBO[3].Create(GL);
            //objVBO[3].Bind(GL);
            //objVBO[3].SetData(GL, VertexAttributes.Instance.AttrbColor, vColor, false, 3);

            objVAO.Unbind(GL);

            //End of example
            importer.Dispose();

            #endregion

        }

        void FindMaxMin(Vector3D data)
        {
            if (MinX > data.X)
                MinX = data.X;
            if (MinY > data.Y)
                MinY = data.Y;
            if (MinZ > data.Z)
                MinZ = data.Z;

            if (MaxX < data.X)
                MaxX = data.X;
            if (MaxY < data.Y)
                MaxY = data.Y;
            if (MaxZ < data.Z)
                MaxZ = data.Z;
        }

        // Render all the meshes. We need to do this because each mesh may need to be bound to a different texture element
        public void RenderObj(OpenGL GL){

            objVAO.Bind(GL);

            for (int i = 0; i < _numMeshes; i++)
            {
                TextureSlot tSlot = new TextureSlot();
                TexContainer tc;
                if (model.Materials[model.Meshes[i].MaterialIndex].GetMaterialTexture(TextureType.Diffuse, 0, out tSlot))
                {
                    tc = TextureManager.Instance.GetElement(AppDomain.CurrentDomain.BaseDirectory + "mesh\\texture\\" + tSlot.FilePath);
                    tc.Tex.Bind(GL); // Bind to the current texture on texture unit 0
                    GL.ActiveTexture(OpenGL.GL_TEXTURE0 + (uint)tc.ID);
                    GL.Uniform1(Uniforms.Instance.Sampler, (int)tc.ID); // it's very important that the datatype matches the signature. If you dont have the cast there, the texture wont load!

                }
                else
                {

                    tc = TextureManager.Instance.GetElement(C_DUMMY_TEXTURE);
                    tc.Tex.Bind(GL); // Bind to the current texture on texture unit 0
                    GL.ActiveTexture(OpenGL.GL_TEXTURE0 + (uint)tc.ID);
                    GL.Uniform1(Uniforms.Instance.Sampler, (int)tc.ID); // it's very important that the datatype matches the signature. If you dont have the cast there, the texture wont load!

                }
                GL.DrawArrays(OpenGL.GL_TRIANGLES, /* Start Index in the buffer */ _meshPointer[i].Start
    , _meshPointer[i].End - _meshPointer[i].Start /*Count of vertices */);
                
            }
            objVAO.Unbind(GL);

        }


    }
}
