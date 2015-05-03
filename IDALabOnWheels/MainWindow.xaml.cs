using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpGL.SceneGraph;
using SharpGL;
using SharpGL.SceneGraph.Assets;
using SharpGL.VertexBuffers;
using GlmNet;
using SharpGL.Shaders;
using System.Drawing;
using System.Diagnostics;
using DR = System.Drawing;
using System.ComponentModel;

namespace IDALabOnWheels
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //  The vertex buffer array which contains the vertex and colour buffers.
        VertexBufferArray vertexBufferArray;

        //  The shader program for our vertex and fragment shader.
        private ShaderProgram shaderProgram;
        ShaderProgram lightingShader;
        string[] vertexShaderSource;
        string[] fragmentShaderSource;
        //  The projection, view and model matrices.
        mat4 projectionMatrix;
        mat4 viewMatrix;
        mat4 modelMatrix;
        mat4 normalMatrix; // Matrix to set the location of surface normals in relation to the light source

        uint attribute_vcol;
        uint attribute_vpos;
        uint attribute_vtexture;
        uint attribute_vSurfNormal;
        uint uniform_mview;
        int uSampler;
        SerialPortAdapter _serialPortAdapter;
        const float C_SAMPLES_PER_FRAME = 2000;

        vec3[] C_PLOT_COLORS = new vec3[] { new vec3(1f, 0f, 0f), new vec3(0f, 1f, 0f), new vec3(0f, 0f, 1f), new vec3(0f, 1f, 1f) };

        vec3[] xgrid;
        vec3[][] _data; // A buffer spanning the entire visible window to store vertices to be plotted. It's a jagged array with one row per sub-plot
        int[] _dataTail; // Pointer to last sample of data in the buffer. Only data until tail will be displayed
        vec3[][] _dataColor;
        int _index;
        static float C_X_MARGIN_MAX = 0.9999f;
        static float C_X_MARGIN_MIN = -0.9999f;
        static float C_Y_MARGIN_MAX = 0.9999f;
        static float C_Y_MARGIN_MIN = -0.9999f;
        static int C_NUM_Y_DIV = 3;
        const int C_SUB_PLOTS = 3;
        static float C_Y_STEP = (C_Y_MARGIN_MAX - C_Y_MARGIN_MIN) / C_NUM_Y_DIV;

        vec3[] cubeSurfaceNormals; // Normals for each vertex of the cube
        vec3[] coldata;
        Bitmap myBitmap;
        SkyBox skyBox;

        ObjModelLoader ObjModel;
        EWBSensorBoard EWBBoard;

        int C_SCENE_DYNAMIC_ELEMENT_COUNT = 2;
        mat4[] ModelMatrix;
        mat4[] ViewMatrix;
        float _modelScaleFactor; // A scaling factor that depends on the obj model being loaded. TODO: Size the model automatically by finding the max/min coordinates
        float _modelYAxixRotFactor; // similar roation

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            MainVM.PropertyChanged += Event_PropertyChanged;
            MainVM.SetupDefaultGUI();

            ObjModel = new ObjModelLoader();
            ModelMatrix = new mat4[C_SCENE_DYNAMIC_ELEMENT_COUNT];
            ViewMatrix = new mat4[C_SCENE_DYNAMIC_ELEMENT_COUNT];

            vertexShaderSource = new string[2];
            fragmentShaderSource = new string[2];
        }

        /// <summary>
        /// Handles changes in the model, reflecting this change in the UI (view)
        /// Callback whenever UI properties change
        /// </summary>
        private void Event_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RotateWorld")
            {
                //UpdateModelParams(_currentModel);
            }

        }

        /// <summary>
        /// Convert Degree to Radians. GLM.NET only works with Radians
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        float D2R(float degree)
        {
            return (degree * (float)Math.PI / 180);
        }

        vec3[] DrawCircle(float cx, float cy, float r, int num_segments)
        {
            vec3[] circle_v = new vec3[num_segments];
            for (int ii = 0; ii < num_segments; ii++)
            {
                float theta = 2.0f * 3.1415926f * ii / num_segments;//get the current angle 
                circle_v[ii].x = r * (float)Math.Cos(theta);//calculate the x component 
                circle_v[ii].y = r * (float)Math.Sin(theta);//calculate the y component 
            }
            return (circle_v);
        }

        float rotation = 0;
        /// <summary>
        /// Handles the OpenGLDraw event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;
            // gl.Viewport(0, 0, (int)openGLControl.Width, (int)openGLControl.Height); // Set up the view port, which is the size of the control here.
            //  Clear the color and depth buffer.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            gl.ClearColor(0.4f, 0.6f, 0.9f, 0.0f);
            //  Load the identity matrix.
            //Teapot tp = new Teapot();
            //tp.Draw(gl, 14, 1, OpenGL.GL_FILL);
            //  Load the scene.
            //Scene scene = SerializationEngine.Instance.LoadScene("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\monkey.obj");
            //scene.Draw();
            //Scene scene = SerializationEngine.Instance.LoadScene("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\Simba.obj");
            //scene.Draw();
            //CreateAndDrawCircle(gl);
            // CreateAndDrawCube(gl, 0);
            //CreateAndDrawCube(gl, 1);
            // DrawObject(gl);

            DrawSkyBox(gl);

            DrawObj(gl);



            // gl.DrawText(5, 50, 1.0f, 0.0f, 0.0f, "Courier New", 12.0f,string.Format("God is Great"));
            // gl.Flush();
            // openGLControl.InvalidateVisual(); // Force a re-draw of the control

            //// Move Left And Into The Screen
            //gl.LoadIdentity();
            //gl.Translate(0.0f, 0.0f, 0.0f);


            //gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);

            //Teapot tp = new Teapot();
            //tp.Draw(gl, 14, 1, OpenGL.GL_FILL);

            //rotation += 3.0f;

            //return;


        }

        Dictionary<string, Texture> TextureList = new Dictionary<string, Texture>();
        Bitmap TexBitmaps;
        Texture ObjTextures;

        /// <summary>
        /// Handles the OpenGLInitialized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            //  TODO: Initialise OpenGL here.
            //  Get the OpenGL object.
            OpenGL gl = openGLControl.OpenGL;

            //gl.Enable(OpenGL.GL_DEPTH_TEST);

            //float[] global_ambient = new float[] { 0.5f, 0.5f, 0.5f, 1.0f };
            //float[] light0pos = new float[] { 0.0f, 5.0f, 10.0f, 1.0f };
            //float[] light0ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            //float[] light0diffuse = new float[] { 0.3f, 0.3f, 0.3f, 1.0f };
            //float[] light0specular = new float[] { 0.8f, 0.8f, 0.8f, 1.0f };

            //float[] lmodel_ambient = new float[] { 0.2f, 0.2f, 0.2f, 1.0f };
            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, lmodel_ambient);

            //gl.LightModel(OpenGL.GL_LIGHT_MODEL_AMBIENT, global_ambient);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, light0pos);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, light0ambient);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, light0diffuse);
            //gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, light0specular);
            //gl.Enable(OpenGL.GL_LIGHTING);
            //gl.Enable(OpenGL.GL_LIGHT0);

            //gl.ShadeModel(OpenGL.GL_SMOOTH);
            //return;


            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);
            //  Set a blue clear colour.
            // gl.ClearColor(0.4f, 0.6f, 0.9f, 0.0f);
            //gl.ClearColor(0f, 0f, 0f, 0.0f);
            //  Create the shader program.
             vertexShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\vertex_shader.glsl");
             fragmentShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\fragment_shader.glsl");
            shaderProgram = new ShaderProgram();
            shaderProgram.Create(gl, vertexShaderSource[0], fragmentShaderSource[0], null);
            attribute_vpos = (uint)gl.GetAttribLocation(shaderProgram.ShaderProgramObject, "vPosition");
            attribute_vcol = (uint)gl.GetAttribLocation(shaderProgram.ShaderProgramObject, "vColor");
            attribute_vtexture = (uint)gl.GetAttribLocation(shaderProgram.ShaderProgramObject, "vTextureCoord");
            attribute_vSurfNormal = (uint)gl.GetAttribLocation(shaderProgram.ShaderProgramObject, "vSurfaceNormal");
            Uniforms.Instance.Sampler = gl.GetUniformLocation(shaderProgram.ShaderProgramObject, "uSampler");
            Uniforms.Instance.SunlightColor = gl.GetUniformLocation(shaderProgram.ShaderProgramObject, "sunLight.vColor");
            Uniforms.Instance.SunlightAmbientIntensity = gl.GetUniformLocation(shaderProgram.ShaderProgramObject, "sunLight.fAmbientIntensity");
            Uniforms.Instance.SunlightDirection = gl.GetUniformLocation(shaderProgram.ShaderProgramObject, "sunLight.vDirection");

            VertexAttributes.Instance.AttrbPosition = attribute_vpos;
            VertexAttributes.Instance.AttrbColor = attribute_vcol;
            VertexAttributes.Instance.AttrbSurfaceNormal = attribute_vSurfNormal;
            VertexAttributes.Instance.AttrbTexture = attribute_vtexture;
            shaderProgram.AssertValid(gl);

            
            InitializeFixedBufferContents();
            //poly = ObjFileReader.ReadFile("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\Simba.obj");

            //xgrid = new vec3[poly.Faces.Count * 4];
            //int i = 0;
            //// go through all the faces of the poly and get the vertices
            //foreach (SharpGL.SceneGraph.Face face in poly.Faces)
            //{
            //    foreach (Index index in face.Indices)
            //    {
            //        float[] temp = poly.Vertices[index.Vertex];
            //        xgrid[i++] = new vec3(temp[0], temp[1], temp[2]);
            //    }
            //}

            //// Add color for each of the 24 vertices defined above
            //coldata = new vec3[xgrid.Length];
            //for (i = 0; i < poly.Vertices.Count; i++)
            //{
            //    coldata[i] = gl.Color(DR.Color.Green);
            //}

            #region ASSIMP MOdel Loading

            //// https://code.google.com/p/assimp-net/wiki/GettingStarted
            ////Create a new importer
            //AssimpContext importer = new AssimpContext();

            ////This is how we add a configuration (each config is its own class)
            //NormalSmoothingAngleConfig config = new NormalSmoothingAngleConfig(66.0f);
            //importer.SetConfig(config);

            ////This is how we add a logging callback 
            //LogStream logstream = new LogStream(delegate(String msg, String userData)
            //{
            //    Console.WriteLine(msg);
            //});
            //logstream.Attach();

            ////Import the model. All configs are set. The model
            ////is imported, loaded into managed memory. Then the unmanaged memory is released, and everything is reset.
            //Assimp.Scene model = importer.ImportFile("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\Wolf\\Wolf.obj", PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.GenerateUVCoords);

            ////TODO: Load the model data into your own structures
            //// http://www.mbsoftworks.sk/index.php?page=tutorials&series=1&tutorial=23
            //// find the total number of faces in the object * 3 for number of vertices
            //// Assume that a face is always a triangle. OBJ files may contain Quad faces, but ASSIMP will convert them into triangles to make our life easier
            //int faces = 0;
            //for (int j = 0; j < model.MeshCount; j++) // for each mesh
            //{
            //    faces += model.Meshes[j].FaceCount; // find the total number of faces in the entire figure (= sum of faces for each mesh)
            //}
            //xgrid = new vec3[faces * 3];
            //// go through all the faces of the poly and get the vertices
            //int i = 0;
            //for (int j = 0; j < model.MeshCount; j++) // for each mesh
            //{
            //    for (int k = 0; k < model.Meshes[j].FaceCount; k++) // for each face in a mesh
            //    {
            //        for (int q = 0; q < model.Meshes[j].Faces[k].IndexCount; q++) // for each index in a face
            //        {
            //            Vector3D temp = model.Meshes[j].Vertices[model.Meshes[j].Faces[k].Indices[q]];
            //            xgrid[i++] = new vec3(temp.X, temp.Y, temp.Z);

            //        }
            //    }
            //    var texC = model.Meshes[j].HasTextureCoords(0);
            //}

            //// Load the materials/textures used for the OBJ
            //int materials = 0;
            //for (int j = 0; j < model.MaterialCount; j++) // for each material in the OBJ
            //{
            //    TextureSlot tSlot = new TextureSlot();
            //    if (model.Materials[j].GetMaterialTexture(TextureType.Diffuse, 0, out tSlot))
            //    {
            //        // Got a texture, check if it's already there in the dictionary. If not, add it
            //        gl.Enable(OpenGL.GL_TEXTURE_2D);
            //        if (!TextureList.ContainsKey(tSlot.FilePath))
            //        {
            //            Debug.Write("Found New Texture!");
            //            TexBitmaps = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "mesh\\texture\\"+ tSlot.FilePath.Split('.')[0] + ".png");
            //            ObjTextures = new Texture();
            //            gl.ActiveTexture(OpenGL.GL_TEXTURE0 + 0);
            //            ObjTextures.Create(gl, TexBitmaps);
            //            ObjTextures.Bind(gl);
            //            TextureList.Add(tSlot.FilePath, ObjTextures);
            //        }

            //    }
            //}


            //// Add color for each of the vertices defined above
            //coldata = new vec3[xgrid.Length];
            //for (i = 0; i < xgrid.Length; i++)
            //{
            //    coldata[i] = gl.Color(DR.Color.Green);
            //}

            ////End of example
            //importer.Dispose();

            //// To render call --> DrawObject(gl);

            #endregion

            skyBox = new SkyBox();
            skyBox.loadSkybox(gl, null);

            //ObjModel.LoadObj(AppDomain.CurrentDomain.BaseDirectory + "mesh\\Wolf.obj", gl); _modelScaleFactor = 3f;
            //ObjModel.LoadObj(AppDomain.CurrentDomain.BaseDirectory + "mesh\\simba.obj", gl); _modelScaleFactor = 1f; _modelYAxixRotFactor = 215f;
            ObjModel.LoadObj(AppDomain.CurrentDomain.BaseDirectory + "mesh\\Assembly.STL", gl); _modelScaleFactor = 1f; _modelYAxixRotFactor = 0;
            InitMatrices();
        }

        float rotation2 = 0;

        void InitMatrices()
        {
            //  Create a model matrix to make the model a little bigger.
            ModelMatrix[1] = mat4.identity();
            ModelMatrix[0] = mat4.identity();
            //ModelMatrix[0] = glm.translate(ModelMatrix[0], new vec3(0f, -5f, 0f));
            ModelMatrix[0] = glm.translate(ModelMatrix[0], new vec3(-ObjModel.Centroid.x, -ObjModel.Centroid.y, -ObjModel.Centroid.z));
           // ModelMatrix[0] = glm.translate(ModelMatrix[0], new vec3(0f, 0f, ObjModel.Centroid.z));
            ModelMatrix[0] = glm.rotate(ModelMatrix[0], D2R(_modelYAxixRotFactor), new vec3(0f, 1f, 0f));
            ModelMatrix[0] = glm.scale(ModelMatrix[0], new vec3(_modelScaleFactor));
           // normalMatrix = myGLM.transpose(new mat4(new vec4(1f, 2f, 3f, 4f), new vec4(5f, 6f, 7f, 8f), new vec4(9f, 10f, 11f, 12f), new vec4(13f, 14f, 15f, 16f)));
            normalMatrix = myGLM.transpose(glm.inverse(ModelMatrix[0]));

            ViewMatrix[0] = glm.lookAt(new vec3(0.0f, 0.0f, ObjModel.Centroid.z * -3), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));
            ViewMatrix[1] = mat4.identity();
        }

        void InitObj()
        {

        }
        void SetMatrices(int index)
        {

            if (index == 0)
            {
                //  Create a perspective projection matrix.
                projectionMatrix = glm.perspective(D2R(60), (float)Width / (float)Height, 0.01f, 100.0f);
                float aspectRatio = (float)(Width / Height);
                float viewSize = 100f;
                // projectionMatrix = glm.ortho(-aspectRatio * viewSize/2 , aspectRatio * viewSize/2, viewSize/2, viewSize/2, -1000, 1000);
                // projectionMatrix = glm.ortho((float)Width, (float)Height, (float)Width, (float)Height);
                //  Create a view matrix to move us back a bit.
                //ViewMatrix[0] = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -1.0f));
                //viewMatrix =  glm.lookAt(new vec3(0.0f, 0.0f, 5.0f), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));



                //  Nudge the rotation.
                rotation += 1f;
              //  ViewMatrix[0] = glm.lookAt(new vec3(0.0f, 0.0f, ObjModel.Centroid.z * -3), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));
              //  ViewMatrix[0] = glm.rotate(ViewMatrix[0], D2R(rotation), new vec3(1f, 0f, 0f));
                
                if (!MainVM.RotateWorld)
                {
                    Attitude att = null;

                    if (EWBBoard != null) att = EWBBoard.GetAverageAttitude();

                    if (att != null)
                    {
                        Debug.WriteLine("Attitude: X = {0}, Y = {1}, Head = {2}", att.angleX, att.angleY, att.heading);

                        // Conceptually operations need to be performed in the order - Scale, Rotate and Translate
                        // However the order of matrix multiplication is reversed - so matrices must be multiplied in the sequence - Translate, Rotate and Scale
                        // to get Scale, Rotate and Translate effect on the actual data.

                        ModelMatrix[0] = mat4.identity();
                        ViewMatrix[0] = glm.lookAt(new vec3(0.0f, 0.0f, ObjModel.Centroid.z * -3), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));
                        //ModelMatrix[0] = glm.translate(ModelMatrix[0], new vec3(0f, -5f, 0f));
                        ModelMatrix[0] = glm.translate(ModelMatrix[0], -1 * ObjModel.Centroid);
                        // Pitch = rotate about Z axes
                        ViewMatrix[0] = glm.rotate(ViewMatrix[0], D2R(att.angleX), new vec3(0f, 0f, 1f));
                        // Roll = rotate about X axes
                        ViewMatrix[0] = glm.rotate(ViewMatrix[0], D2R(att.angleY), new vec3(1f, 0f, 0f));
                        // Yaw = rotate about Y axis
                        ViewMatrix[0] = glm.rotate(ViewMatrix[0], D2R(att.heading), new vec3(0f, 1f, 0f));
                        //  Create a model matrix to make the model a little bigger.
                        ModelMatrix[0] = glm.scale(ModelMatrix[0], new vec3(_modelScaleFactor));

                        
                        //normalMatrix = glm.scale(modelMatrix, new vec3(1f)); ;

                        // ModelMatrix[0] = glm.scale(new mat4(1.0f), new vec3(4f));
                    }
                }


                // modelMatrix = glm.scale(new mat4(1.0f), new vec3(0.03f));
                // modelMatrix = glm.rotate(modelMatrix, D2R(rotation), new vec3(0f, 0f, 1f));



                //viewMatrix = viewMatrix * modelMatrix;
                //modelMatrix = mat4.identity();
            }
            else
            {
                projectionMatrix = glm.perspective(D2R(60), (float)Width / (float)Height, 0.01f, 100.0f);
                viewMatrix = glm.translate(new mat4(1.0f), new vec3(5.0f, 0.0f, -5.0f));
                //  Nudge the rotation.
                rotation2 += 20f;
                //  Create a model matrix to make the model a little bigger.
                modelMatrix = glm.scale(new mat4(1.0f), new vec3(0.6f));
                modelMatrix = glm.rotate(modelMatrix, D2R(rotation2), new vec3(.5f, .5f, 0));
            }
            normalMatrix = myGLM.transpose(glm.inverse(ViewMatrix[0]));
            //normalMatrix = glm.inverse(modelMatrix);
           // normalMatrix = glm.scale(modelMatrix, new vec3(1f)); ;

        }



        // render an OBJ object
        void DrawObject(OpenGL GL)
        {
            SetMatrices(0);




            ////  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(GL);
            vertexBufferArray.Bind(GL);

            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(GL);
            vertexDataBuffer.Bind(GL);
            vertexDataBuffer.SetData(GL, attribute_vpos, xgrid, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(GL);
            colourDataBuffer.Bind(GL);
            colourDataBuffer.SetData(GL, attribute_vcol, coldata, false, 3);

            ////  Now do the same for the texture map data.
            //var texturerDataBuffer = new VertexBuffer();
            //texturerDataBuffer.Create(GL);
            //texturerDataBuffer.Bind(GL);
            //texturerDataBuffer.SetData(GL, attribute_vtexture, textureCoordinates, false, 2);

            //  Now do the same for the texture map data.
            //var lightingDataBuffer = new VertexBuffer();
            //texturerDataBuffer.Create(GL);
            //texturerDataBuffer.Bind(GL);
            //texturerDataBuffer.SetData(GL, attribute_vSurfNormal, cubeSurfaceNormals, false, 3);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(GL);

            //  Bind the shader, set the matrices.
            shaderProgram.Bind(GL);
            shaderProgram.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "normalMatrix", normalMatrix.to_array());

            //  Bind the out vertex array.
            vertexBufferArray.Bind(GL);

            _texture = new Texture();

            // Open a Stream and decode a PNG image
            //GL.ActiveTexture(OpenGL.GL_TEXTURE0);
            //myBitmap = new Bitmap("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\1.png");
            //_texture.Create(GL, myBitmap);
            //_texture.Bind(GL);
            //shaderProgram.SetUniform1(GL, "uSampler", 0);

            GL.DrawArrays(OpenGL.GL_TRIANGLES, 0, xgrid.Length);
            // 6 triangle vertices in a face, 36 vertices in the whole cube
            //GL.DrawElements(OpenGL.GL_TRIANGLES, 36, cubeVertexIndices); // Use elements to save RAM

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(GL);
            shaderProgram.Unbind(GL);

        }


        void CreateAndDrawCube(OpenGL GL, int index)
        {
            // Create a Cube with 24 vertices. When you split a cube into triangles you will get 36 vertices
            xgrid = new vec3[] {    // Front face
                                    new vec3(-1f, -1.0f, 1.0f), 
                                    new vec3(1.0f, -1.0f, 1.0f), 
                                    new vec3(1.0f, 1.0f, 1.0f), 
                                    new vec3(-1.0f, 1.0f, 1.0f),
                                    // Back face
                                    new vec3(-1f, -1.0f, -1.0f), 
                                    new vec3(-1.0f, 1.0f, -1.0f), 
                                    new vec3(1.0f, 1.0f, -1.0f), 
                                    new vec3(1.0f, -1.0f, -1.0f),
                                    // Top face
                                    new vec3(-1f, 1.0f, -1.0f), 
                                    new vec3(-1.0f, 1.0f, 1.0f), 
                                    new vec3(1.0f, 1.0f, 1.0f), 
                                    new vec3(1.0f, 1.0f, -1.0f),
                                    // Bottom face
                                    new vec3(-1f, -1.0f, -1.0f), 
                                    new vec3(1.0f, -1.0f, -1.0f), 
                                    new vec3(1.0f, -1.0f, 1.0f), 
                                    new vec3(-1.0f, -1.0f, 1.0f),
                                    // Right face
                                    new vec3(1f, -1.0f, -1.0f), 
                                    new vec3(1.0f, 1.0f, -1.0f), 
                                    new vec3(1.0f, 1.0f, 1.0f), 
                                    new vec3(1.0f, -1.0f, 1.0f),
                                    // Left face
                                    new vec3(-1f, -1.0f, -1.0f), 
                                    new vec3(-1.0f, -1.0f, 1.0f), 
                                    new vec3(-1.0f, 1.0f, 1.0f), 
                                    new vec3(-1.0f, 1.0f, -1.0f),
            };

            cubeSurfaceNormals = new vec3[] { 
            // Front
     new vec3(0.0f,  0.0f,  1.0f),
     new vec3(0.0f,  0.0f,  1.0f),
     new vec3(0.0f,  0.0f,  1.0f),
    new vec3(0.0f,  0.0f,  1.0f),
    
    // Back
     new vec3(0.0f,  0.0f, -1.0f),
    new vec3(0.0f,  0.0f, -1.0f),
     new vec3(0.0f,  0.0f, -1.0f),
     new vec3(0.0f,  0.0f, -1.0f),
    
    // Top
    new vec3( 0.0f,  1.0f,  0.0f),
     new vec3( 0.0f,  1.0f,  0.0f),
    new vec3( 0.0f,  1.0f,  0.0f),
     new vec3( 0.0f,  1.0f,  0.0f),
    
    // Bottom
     new vec3(0.0f, -1.0f,  0.0f),
     new vec3(0.0f, -1.0f,  0.0f),
    new vec3(0.0f, -1.0f,  0.0f),
     new vec3(0.0f, -1.0f,  0.0f),
    
    // Right
     new vec3(1.0f,  0.0f,  0.0f),
     new vec3(1.0f,  0.0f,  0.0f),
     new vec3(1.0f,  0.0f,  0.0f),
     new vec3(1.0f,  0.0f,  0.0f),
    
    // Left
    new vec3(-1.0f,  0.0f,  0.0f),
    new vec3(-1.0f,  0.0f,  0.0f),
    new vec3(-1.0f,  0.0f,  0.0f),
   new vec3(-1.0f,  0.0f,  0.0f)};

            var cubeVertexIndices = new uint[] {
  0, 1, 2, 0, 2, 3,    // front
  4, 5, 6, 4, 6, 7,    // back
  8, 9, 10, 8, 10, 11,   // top
  12, 13, 14, 12, 14, 15,   // bottom
  16, 17, 18, 16, 18, 19,   // right
  20, 21, 22, 20, 22, 23    // left
            };

            var textureCoordinates = new vec2[] {
    // Front
    new vec2(0f, 0f), 
    new vec2(2f, 0f), 
    new vec2(2f, 2f), 
    new vec2(0f, 2f), 
    // Back
        new vec2(0f, 0f), 
    new vec2(1f, 0f), 
    new vec2(1f, 1f), 
    new vec2(0f, 1f), 

    // Top
        new vec2(0f, 0f), 
    new vec2(1f, 0f), 
    new vec2(1f, 1f), 
    new vec2(0f, 1f), 

    // Bottom
        new vec2(0f, 0f), 
    new vec2(1f, 0f), 
    new vec2(1f, 1f), 
    new vec2(0f, 1f), 

    // Right
        new vec2(0f, 0f), 
    new vec2(1f, 0f), 
    new vec2(1f, 1f), 
    new vec2(0f, 1f), 

    // Left
        new vec2(0f, 0f), 
    new vec2(1f, 0f), 
    new vec2(1f, 1f), 
    new vec2(0f, 1f), 

  };

            SetMatrices(index);

            // The color of each face
            vec3[] faceColor = new vec3[] { 
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.Red),
                GL.Color(DR.Color.Green),
                GL.Color(DR.Color.Blue) ,
                GL.Color(DR.Color.Yellow),
                GL.Color(DR.Color.Purple)
            };

            // Add color for each of the 24 vertices defined above
            coldata = new vec3[xgrid.Length];
            for (int i = 0; i < faceColor.Length; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    coldata[i * 4 + j] = faceColor[i];
                }
            }

            ////  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(GL);
            vertexBufferArray.Bind(GL);

            //  Create a vertex buffer for the vertex data.
            //var indexDataBuffer = new IndexBuffer();
            //indexDataBuffer.Create(GL);
            //indexDataBuffer.Bind(GL);
            //indexDataBuffer.SetData(GL, cubeVertexIndices);
            //vertexDataBuffer.SetData(GL, attribute_vpos, xgrid, false, 3);

            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(GL);
            vertexDataBuffer.Bind(GL);
            vertexDataBuffer.SetData(GL, attribute_vpos, xgrid, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(GL);
            colourDataBuffer.Bind(GL);
            colourDataBuffer.SetData(GL, attribute_vcol, coldata, false, 3);

            //  Now do the same for the texture map data.
            var texturerDataBuffer = new VertexBuffer();
            texturerDataBuffer.Create(GL);
            texturerDataBuffer.Bind(GL);
            texturerDataBuffer.SetData(GL, attribute_vtexture, textureCoordinates, false, 2);

            //  Now do the same for the texture map data.
            var lightingDataBuffer = new VertexBuffer();
            texturerDataBuffer.Create(GL);
            texturerDataBuffer.Bind(GL);
            texturerDataBuffer.SetData(GL, attribute_vSurfNormal, cubeSurfaceNormals, false, 3);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(GL);

            //  Bind the shader, set the matrices.
            shaderProgram.Bind(GL);
            shaderProgram.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "normalMatrix", normalMatrix.to_array());

            //  Bind the out vertex array.
            vertexBufferArray.Bind(GL);

            _texture = new Texture();

            // Open a Stream and decode a PNG image
            GL.ActiveTexture(OpenGL.GL_TEXTURE1);
            myBitmap = new Bitmap("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\1.png");
            _texture.Create(GL, myBitmap);
            _texture.Bind(GL);
            //shaderProgram.SetUniform1(GL, "uSampler", 1);
            GL.Uniform1(uSampler, 1);

            //GL.DrawArrays(OpenGL.GL_TRIANGLES, 0, 36);
            // 6 triangle vertices in a face, 36 vertices in the whole cube
            GL.DrawElements(OpenGL.GL_TRIANGLES, 36, cubeVertexIndices); // Use elements to save RAM

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(GL);
            shaderProgram.Unbind(GL);
        }

        Texture _texture;
        void CreateAndDrawCircle(OpenGL GL)
        {
            SetMatrices(1);
            xgrid = DrawCircle(0f, 0f, 1f, 100);
            coldata = new vec3[xgrid.Length];
            for (int i = 0; i < xgrid.Length; i++)
            {
                coldata[i] = GL.Color(DR.Color.ForestGreen);
            }
            ////  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(GL);
            vertexBufferArray.Bind(GL);

            //  Create a vertex buffer for the vertex data.
            //var indexDataBuffer = new IndexBuffer();
            //indexDataBuffer.Create(GL);
            //indexDataBuffer.Bind(GL);
            //indexDataBuffer.SetData(GL, cubeVertexIndices);
            //vertexDataBuffer.SetData(GL, attribute_vpos, xgrid, false, 3);

            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(GL);
            vertexDataBuffer.Bind(GL);
            vertexDataBuffer.SetData(GL, attribute_vpos, xgrid, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(GL);
            colourDataBuffer.Bind(GL);
            colourDataBuffer.SetData(GL, attribute_vcol, coldata, false, 3);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(GL);

            //  Bind the shader, set the matrices.
            shaderProgram.Bind(GL);
            shaderProgram.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());

            //  Bind the out vertex array.
            vertexBufferArray.Bind(GL);

            //_texture = new Texture();

            //// Open a Stream and decode a PNG image
            //GL.ActiveTexture(OpenGL.GL_TEXTURE0);
            //Bitmap myBitmap = new Bitmap("D:\\OpenGL\\SharpGLWPFPlot\\SharpGLWPFPlot\\Resources\\1.png");
            //_texture.Create(GL, myBitmap);
            //_texture.Bind(GL);
            //shaderProgram.SetUniform1(GL, "uSampler", 0);

            GL.DrawArrays(OpenGL.GL_TRIANGLE_FAN, 0, 100);
            // 6 triangle vertices in a face, 36 vertices in the whole cube
            //GL.DrawElements(OpenGL.GL_TRIANGLES, 36, cubeVertexIndices); // Use elements to save RAM

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(GL);
            shaderProgram.Unbind(GL);
        }

        void CreateAndDrawGrid(OpenGL GL)
        {
            // Debug.WriteLine("Painting Begins..");

            // Create vertices for 4 lines that will split the figure into 3 equal sections
            xgrid = new vec3[(C_NUM_Y_DIV + 1) * 2];
            for (int i = 0; i < (C_NUM_Y_DIV + 1) * 2; i = i + 2)
            {
                xgrid[i].x = -1f; xgrid[i + 1].x = 1f;
                xgrid[i].y = C_X_MARGIN_MIN + C_Y_STEP * i / 2; xgrid[i + 1].y = C_X_MARGIN_MIN + C_Y_STEP * i / 2;
                xgrid[i].z = 0f; xgrid[i + 1].z = 0f;
            }

            coldata = new vec3[] { 
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White),
                GL.Color(DR.Color.White)
            };


            // --------------- Implementation WITH VERTEX ARRAY OBJECTS --------------------
            ////  Create the vertex array object.
            vertexBufferArray = new VertexBufferArray();
            vertexBufferArray.Create(GL);
            vertexBufferArray.Bind(GL);

            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(GL);
            vertexDataBuffer.Bind(GL);
            vertexDataBuffer.SetData(GL, attribute_vpos, xgrid, false, 3);

            //  Now do the same for the colour data.
            var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(GL);
            colourDataBuffer.Bind(GL);
            colourDataBuffer.SetData(GL, attribute_vcol, coldata, false, 3);

            //  Unbind the vertex array, we've finished specifying data for it.
            vertexBufferArray.Unbind(GL);

            //  Bind the shader, set the matrices.
            shaderProgram.Bind(GL);
            //shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            //shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "modelview", modelMatrix.to_array());

            //  Bind the out vertex array.
            vertexBufferArray.Bind(GL);

            GL.DrawArrays(OpenGL.GL_LINES, 0, 8);

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(GL);
            shaderProgram.Unbind(GL);

            // --------------- Implementation WITH VERTEX ARRAY OBJECTS END --------------------

        }


        

        /// <summary>
        /// Since the X cordinates are fixed for each sample in the buffer and just depends on the actual size of the buffer, they can be initialized once and for all.
        /// </summary>
        void InitializeFixedBufferContents()
        {
            // Allocate memory
            _dataTail = new int[C_SUB_PLOTS];
            _data = new vec3[C_SUB_PLOTS][]; // Vertices
            _dataColor = new vec3[C_SUB_PLOTS][]; // Color for each vertex

            for (int j = 0; j < C_SUB_PLOTS; j++)
            {
                _data[j] = new vec3[(int)C_SAMPLES_PER_FRAME];
                _dataColor[j] = new vec3[(int)C_SAMPLES_PER_FRAME];

                for (int i = 0; i < C_SAMPLES_PER_FRAME; i++)
                {
                    _data[j][i].x = 2 * (i - 0) / (C_SAMPLES_PER_FRAME - 0) - 1;

                    // Set the color for each vertex
                    _dataColor[j][i] = C_PLOT_COLORS[j];
                }
            }
        }

        void DrawObj(OpenGL GL)
        {

            //viewMatrix = mat4.identity();
            //projectionMatrix = mat4.identity();
            //projectionMatrix = glm.perspective(D2R(60), (float)Width / (float)Height, 0.01f, 100.0f);
            ////projectionMatrix = glm.ortho( -100f, 100f, -100f, 100f,-100f, 100f);
            //modelMatrix = mat4.identity();
            //normalMatrix = mat4.identity();
            //viewMatrix = glm.lookAt(new vec3(0f, 0f, 0f), new vec3(0f, 0f, 100f), new vec3(0.0f, 1.0f, 0.0f));
            SetMatrices(0);
            //normalMatrix = myGLM.transpose(glm.inverse(ModelMatrix[0]));
            shaderProgram.Bind(GL);
            shaderProgram.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "viewMatrix", ViewMatrix[0].to_array());
            shaderProgram.SetUniformMatrix4(GL, "modelMatrix", ModelMatrix[0].to_array());
            shaderProgram.SetUniformMatrix4(GL, "normalMatrix", normalMatrix.to_array());

            GL.Uniform3(Uniforms.Instance.SunlightColor, 1f,1f,1f);
            GL.Uniform1(Uniforms.Instance.SunlightAmbientIntensity, .55f);
            GL.Uniform3(Uniforms.Instance.SunlightDirection, 0f, 0f, -1f);

            ObjModel.RenderObj(GL);

            //modelMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 5.0f, -1.0f));
            //shaderProgram.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());
            //ObjModel.RenderObj(GL);
            shaderProgram.Unbind(GL);
        }

        float theta = -180;
        float phi = 0;
        void DrawSkyBox(OpenGL GL)
        {
            //SetMatrices(0);
            viewMatrix = mat4.identity();
            projectionMatrix = mat4.identity();
            projectionMatrix = glm.perspective(D2R(60), (float)Width / (float)Height, 0.01f, 100.0f);
            //projectionMatrix = glm.ortho( -100f, 100f, -100f, 100f,-100f, 100f);
            modelMatrix = mat4.identity();
            normalMatrix = mat4.identity();

            //theta += 10f;
            //phi += 10f;
            viewMatrix = glm.lookAt(new vec3(0f, 0f, 0f), new vec3((float)Math.Cos(D2R(theta)) * (float)Math.Cos(D2R(phi)), (float)Math.Sin(D2R(theta)) * (float)Math.Cos(D2R(phi)), (float)Math.Sin(D2R(phi))), new vec3(0.0f, 1f, 0f));
            //viewMatrix = glm.lookAt(new vec3(0f, 0f,0f), new vec3(0f, 0f, 100f), new vec3(0.0f, 1.0f, 0.0f));

            // 	gluLookAt(	x, 1.0f, z, x+lx, 1.0f,  z+lz,0.0f, 1.0f,  0.0f);
            float x = 0f;
            float z = 1f;
            rotation += 1f;
            float lx = (float)Math.Sin(D2R(rotation));
            float lz = (float)-Math.Cos(D2R(rotation));
            //ViewMatrix[1] = glm.rotate(mat4.identity(), D2R(rotation), new vec3(0f, 1f, 0f));
            if (MainVM.RotateWorld)
            {
                Attitude att = null;

                if (EWBBoard != null) att = EWBBoard.GetAverageAttitude();

                    if (att != null)
                    {
                        Debug.WriteLine("Attitude: X = {0}, Y = {1}, Head = {2}", att.angleX, att.angleY, att.heading);

                        // Conceptually operations need to be performed in the order - Scale, Rotate and Translate
                        // However the order of matrix multiplication is reversed - so matrices must be multiplied in the sequence - Translate, Rotate and Scale
                        // to get Scale, Rotate and Translate effect on the actual data.

                        ModelMatrix[1] = mat4.identity();
                        ViewMatrix[1] = mat4.identity();
                        // Pitch = rotate about Z axes
                        ViewMatrix[1] = glm.rotate(ViewMatrix[1], D2R(att.angleX), new vec3(0f, 0f, 1f));
                        // Roll = rotate about X axes
                        ViewMatrix[1] = glm.rotate(ViewMatrix[1], D2R(att.angleY), new vec3(1f, 0f, 0f));
                        // Yaw = rotate about Y axis
                        ViewMatrix[1] = glm.rotate(ViewMatrix[1], D2R(att.heading), new vec3(0f, 1f, 0f));
                    }
            }
            normalMatrix = myGLM.transpose(glm.inverse(ModelMatrix[1]));
           // ModelMatrix[1] = mat4.identity();
           // ModelMatrix[1] = glm.translate(ModelMatrix[1], new vec3(0f, 0f, 0.1f));

            //            // Compute new orientation
            //float horizontalAngle=0f,verticalAngle =0f;  
            //   horizontalAngle     +=0.1f;//+= mouseSpeed * float(1024/2 - xpos );
            // verticalAngle +=0.1f;  //+= mouseSpeed * float( 768/2 - ypos );

            //// Direction : Spherical coordinates to Cartesian coordinates conversion
            //vec3 direction= new vec3 (
            //    (float) Math.Cos(verticalAngle) * (float) Math.Sin(horizontalAngle), 
            //    (float) Math.Sin(verticalAngle),
            //    (float) Math.Cos(verticalAngle) * (float) Math.Cos(horizontalAngle)
            //);

            //// Right vector
            //vec3  right = new vec3(
            //    (float) Math.Sin(horizontalAngle - 3.14f/2.0f), 
            //    0,
            //    (float) Math.Cos(horizontalAngle - 3.14f/2.0f)
            //);

            //// Up vector
            //vec3 up = glm.cross( right, direction );

            //vec3 position = new vec3(x, 0f, z);
            //viewMatrix = glm.lookAt(new vec3(x, 0f, z), position + direction, up);
            //viewMatrix = glm.lookAt(new vec3(x, 0f, z), new vec3(x+lx, 0f, z+lz), new vec3(0.0f, 1.0f, 0.0f));
            //viewMatrix = glm.lookAt(new vec3(x + lx, 0f, z + lz), new vec3(0f, 0f, 0f), new vec3(0.0f, 1.0f, 0.0f));
            //viewMatrix = glm.translate(new mat4(1.0f), new vec3(0f, 0f, -10f));
            //viewMatrix = glm.rotate(viewMatrix, D2R(rotation), new vec3(1f, 0f, 0f));
            //  Create a model matrix to make the model a little bigger.
            //modelMatrix = glm.scale(new mat4(1.0f), new vec3(1f));
            //modelMatrix = glm.rotate(modelMatrix, D2R(rotation), new vec3(0f, 0f, 1f));

            shaderProgram.Bind(GL);
            shaderProgram.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            shaderProgram.SetUniformMatrix4(GL, "viewMatrix", ViewMatrix[1].to_array());
            shaderProgram.SetUniformMatrix4(GL, "modelMatrix", ModelMatrix[1].to_array());
            shaderProgram.SetUniformMatrix4(GL, "normalMatrix", normalMatrix.to_array());
            GL.Uniform3(Uniforms.Instance.SunlightColor, 1f, 1f, 1f);
            GL.Uniform1(Uniforms.Instance.SunlightAmbientIntensity, 1f);
            GL.Uniform3(Uniforms.Instance.SunlightDirection, 0f, -1f, 0f);

            skyBox.renderSkybox(GL, shaderProgram);


        }


        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            //  TODO: Set the projection matrix here.

            ////  Get the OpenGL object.
            //OpenGL gl = openGLControl.OpenGL;

            ////  Set the projection matrix.
            //gl.MatrixMode(OpenGL.GL_PROJECTION);

            ////  Load the identity.
            //gl.LoadIdentity();

            ////  Create a perspective transformation.
            //gl.Perspective(60.0f, (double)Width / (double)Height, 0.01, 100.0);

            ////  Use the 'look at' helper function to position and aim the camera.
            //gl.LookAt(-5, 5, -5, 0, 0, 0, 0, 1, 0);

            ////  Set the modelview matrix.
            //gl.MatrixMode(OpenGL.GL_MODELVIEW);
        }


        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            EWBBoard = new EWBSensorBoard();
            if (EWBBoard.Open(cmbxPorts.Text))
            {
                MainVM.StartStopIsEnabled = true;
                Debug.WriteLine("Opened port to :" + cmbxPorts.Text);
                MainVM.BTConnectIsEnabled = false;
            }

        }


        bool _enableStop = false;
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {

            if (_enableStop == false)
            {
                _enableStop = true;
                btnStartStop.Content = "Stop";
                EWBBoard.Start();
            }
            else
            {
                _enableStop = false;
                btnStartStop.Content = "Start";
                EWBBoard.Stop();
            }

        }


        private void Window_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("Application EXIT Triggered..");
            if (EWBBoard != null)
                EWBBoard.DoOnQuit();

            System.Environment.Exit(0);
        }

        private void cbSimulate_Click(object sender, RoutedEventArgs e)
        {

            //if (cbSimulate.IsChecked == true)
            //{
            //    _accelInstance = new Accelerometer();
            //    _serialPortAdapter = new SerialPortAdapter();
            //    _serialPortAdapter.SerialDataRxedHandler = _accelInstance.AccelerometerByteStreamParser;
            //    _serialPortAdapter.Simulate = true;
            //    btnStartStop.IsEnabled = true;
            //    Debug.WriteLine("Entered Simulation Mode!!");
            //    btnConnect.IsEnabled = false;
            //}
            //else
            //{
            //    if (_serialPortAdapter != null) { _serialPortAdapter.Simulate = false; }
            //    else
            //    {
            //        btnStartStop.IsEnabled = false;
            //        btnConnect.IsEnabled = true;
            //    }
            //    Debug.WriteLine("Exit Simulation Mode!!");
            //}
        }
    }
}
