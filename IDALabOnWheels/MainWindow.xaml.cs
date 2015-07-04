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
using System.Windows.Media.Animation;
using MD = System.Windows.Media;

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
        private GShaderProgram textureShader;
        GShaderProgram colorShader;
        GShaderProgram simpleShader;
        string[] vertexShaderSource;
        string[] fragmentShaderSource;
        string[] geomShaderSource;
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
        LinePlot RawAccPlot;
        LinePlot RawMagPlot;
        LinePlot RawGyroPlot;

        int C_SCENE_DYNAMIC_ELEMENT_COUNT = 2;
        mat4[] ModelMatrix;
        mat4[] ViewMatrix;
        float _modelScaleFactor; // A scaling factor that depends on the obj model being loaded. TODO: Size the model automatically by finding the max/min coordinates
        float _modelYAxixRotFactor; // similar roation
        float _cameraZxZoomFactor;
        Compass _compass;
        CompassArrow _cArrow;
        double cWidth;
        double cHeight;

        ActivityTimer _activityTimer;
        StaticActivity _staticActivity;
        DynamicActivity _dynamicActivity;
        bool _simulate = false;
        float _compassRot;

        DR.Color COLOR_ACC_X = DR.Color.Red;
        DR.Color COLOR_ACC_Y = DR.Color.HotPink;
        DR.Color COLOR_ACC_Z = DR.Color.IndianRed;
        DR.Color COLOR_MAG_X = DR.Color.Orange;
        DR.Color COLOR_MAG_Y = DR.Color.Khaki;
        DR.Color COLOR_MAG_Z = DR.Color.Yellow;
        DR.Color COLOR_GYR_X = DR.Color.Green;
        DR.Color COLOR_GYR_Y = DR.Color.DarkMagenta;
        DR.Color COLOR_GYR_Z = DR.Color.Blue;


        // TODO: Graph ?? Temp, Altitude display, Calibrarion buttons, complete dynamic activity
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            // Setup legend colors for plot
            CheckBox item = (CheckBox)LayoutRoot.FindName("AccX");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_ACC_X.A, COLOR_ACC_X.R, COLOR_ACC_X.G, COLOR_ACC_X.B));
            item = (CheckBox)LayoutRoot.FindName("AccY");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_ACC_Y.A, COLOR_ACC_Y.R, COLOR_ACC_Y.G, COLOR_ACC_Y.B));
            item = (CheckBox)LayoutRoot.FindName("AccZ");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_ACC_Z.A, COLOR_ACC_Z.R, COLOR_ACC_Z.G, COLOR_ACC_Z.B));

            item = (CheckBox)LayoutRoot.FindName("MagX");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_MAG_X.A, COLOR_MAG_X.R, COLOR_MAG_X.G, COLOR_MAG_X.B));
            item = (CheckBox)LayoutRoot.FindName("MagY");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_MAG_Y.A, COLOR_MAG_Y.R, COLOR_MAG_Y.G, COLOR_MAG_Y.B));
            item = (CheckBox)LayoutRoot.FindName("MagZ");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_MAG_Z.A, COLOR_MAG_Z.R, COLOR_MAG_Z.G, COLOR_MAG_Z.B));

            item = (CheckBox)LayoutRoot.FindName("Yaw");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_GYR_X.A, COLOR_GYR_X.R, COLOR_GYR_X.G, COLOR_GYR_X.B));
            item = (CheckBox)LayoutRoot.FindName("Pitch");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_GYR_Y.A, COLOR_GYR_Y.R, COLOR_GYR_Y.G, COLOR_GYR_Y.B));
            item = (CheckBox)LayoutRoot.FindName("Roll");
            item.Foreground = new SolidColorBrush(MD.Color.FromArgb(COLOR_GYR_Z.A, COLOR_GYR_Z.R, COLOR_GYR_Z.G, COLOR_GYR_Z.B));

            MainVM.PropertyChanged += Event_PropertyChanged;
            MainVM.SetupDefaultGUI();
            cmbxPorts.SelectedIndex = Properties.Settings.Default.SelectedPort;

            ObjModel = new ObjModelLoader();
            ModelMatrix = new mat4[C_SCENE_DYNAMIC_ELEMENT_COUNT];
            ViewMatrix = new mat4[C_SCENE_DYNAMIC_ELEMENT_COUNT];

            vertexShaderSource = new string[2];
            fragmentShaderSource = new string[2];
            geomShaderSource = new string[2];

            RawAccPlot = new LinePlot();
            RawMagPlot = new LinePlot();
            RawGyroPlot = new LinePlot();
            
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
            if (e.PropertyName == "StaticActivity")
            {
                //UpdateModelParams(_currentModel);
            }
            if (e.PropertyName == "DynamicActivity")
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

            DrawCompass(gl);
            DrawPlot(gl);
            //RawAccPlot.Render(gl, textureShader);

            if (EWBBoard == null) return;

            float data = EWBBoard.GetAverageTemperature();
            if(data != 0f) MainVM.Temperature = data.ToString();

            data = EWBBoard.GetAverageAltitude();
            if(data != 0f) MainVM.Altitude = data.ToString();


            // gl.DrawText(5, 500, 1.0f, 0.0f, 0.0f, "Courier New", 12.0f, DateTime.Now.ToShortTimeString());
            // gl.Flush();

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
            textureShader = new GShaderProgram();
            //vertexShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\vertex_shader.glsl");
            //fragmentShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\fragment_shader.glsl");
            //textureShader.Create(gl, vertexShaderSource[0], fragmentShaderSource[0], null);
            vertexShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\particle.vert");
            fragmentShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\particle.frag");
            geomShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\main_shader.geom");
            textureShader.Create(gl, vertexShaderSource[0], fragmentShaderSource[0], geomShaderSource[0], null);



            attribute_vpos = (uint)gl.GetAttribLocation(textureShader.ShaderProgramObject, "vPosition");
            attribute_vcol = (uint)gl.GetAttribLocation(textureShader.ShaderProgramObject, "vColor");
            attribute_vtexture = (uint)gl.GetAttribLocation(textureShader.ShaderProgramObject, "vTextureCoord");
            attribute_vSurfNormal = (uint)gl.GetAttribLocation(textureShader.ShaderProgramObject, "vSurfaceNormal");
            textureShader.AssertValid(gl);


            InitializeFixedBufferContents();
            skyBox = new SkyBox();
            skyBox.loadSkybox(gl, null, textureShader);

            // ObjModel.LoadObj(AppDomain.CurrentDomain.BaseDirectory + "mesh\\Wolf.obj", gl, textureShader); _modelScaleFactor = 1f;
            //ObjModel.LoadObj(AppDomain.CurrentDomain.BaseDirectory + "mesh\\simba.obj", gl, textureShader); _modelScaleFactor = 1f; _modelYAxixRotFactor = 215f;
            ObjModel.LoadObj(AppDomain.CurrentDomain.BaseDirectory + "mesh\\Assembly.STL", gl, textureShader); _modelScaleFactor = 1f; _modelYAxixRotFactor = 0;

            _compass = new Compass();
            _compass.Create(gl, AppDomain.CurrentDomain.BaseDirectory + "textures\\compass-dial.png", textureShader);
            _cArrow = new CompassArrow();
            _cArrow.Create(gl, AppDomain.CurrentDomain.BaseDirectory + "textures\\compass-arrow.png", textureShader);
            InitMatrices();
            cWidth = openGLControl.ActualWidth;
            cHeight = openGLControl.ActualHeight;
            Debug.WriteIf(_debug, "Width = " + cWidth.ToString() + " Height = " + cHeight.ToString());

            simpleShader = new GShaderProgram();
            vertexShaderSource[1] = ManifestResourceLoader.LoadTextFile("Shaders\\simple.vert");
            fragmentShaderSource[1] = ManifestResourceLoader.LoadTextFile("Shaders\\simple.frag");
            simpleShader.Create(gl, vertexShaderSource[1], fragmentShaderSource[1], null);
            simpleShader.AssertValid(gl);

            DR.Color[] col = new DR.Color[] { COLOR_ACC_X, COLOR_ACC_Y, COLOR_ACC_Z };
            string[] id = new string[] { "accX", "accY", "accZ" };
            RawAccPlot.Initialize(gl, col, -700, 700, id, 3);

            col = new DR.Color[] { COLOR_MAG_X, COLOR_MAG_Y, COLOR_MAG_Z };
            id = new string[] { "magX", "magY", "magZ" };
            RawMagPlot.Initialize(gl, col, -256, 256, id, 3);

            col = new DR.Color[] { COLOR_GYR_X, COLOR_GYR_Y, COLOR_GYR_Z };
            id = new string[] { "gyroX", "gyroY", "gyroZ" };
            RawGyroPlot.Initialize(gl, col, -128, 128, id, 3);
        }

        float rotation2 = 0;

        void InitMatrices()
        {
            _cameraZxZoomFactor = -3f;
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

            ViewMatrix[0] = glm.lookAt(new vec3(0.0f, 0.0f, ObjModel.Centroid.z * _cameraZxZoomFactor), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));
            ViewMatrix[1] = mat4.identity();
        }


        void SetMatrices(int index)
        {

            if (index == 0)
            {
                //  Create a perspective projection matrix.
                projectionMatrix = glm.perspective(D2R(60), (float)cWidth / (float)cHeight, 0.01f, 100.0f);
                float aspectRatio = (float)(Width / Height);
                float viewSize = 100f;
                // projectionMatrix = glm.ortho(-aspectRatio * viewSize/2 , aspectRatio * viewSize/2, viewSize/2, viewSize/2, -1000, 1000);
                // projectionMatrix = glm.ortho((float)Width, (float)Height, (float)Width, (float)Height);
                //  Create a view matrix to move us back a bit.
                //ViewMatrix[0] = glm.translate(new mat4(1.0f), new vec3(0.0f, 0.0f, -1.0f));
                //viewMatrix =  glm.lookAt(new vec3(0.0f, 0.0f, 5.0f), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));



                //  Nudge the rotation.
                rotation += 1f;
                ViewMatrix[0] = glm.lookAt(new vec3(0.0f, 0.0f, ObjModel.Centroid.z * _cameraZxZoomFactor), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));
                //if (!_simulate)
                //{
                //    // ViewMatrix[0] = glm.rotate(ViewMatrix[0], D2R(rotation), new vec3(1f, 0f, 0f));
                //    ModelMatrix[0] = mat4.identity();
                //    // ModelMatrix[0] = glm.translate(ModelMatrix[0], new vec3(ObjModel.Centroid.x, ObjModel.Centroid.y, ObjModel.Centroid.z));
                //    ModelMatrix[0] = glm.scale(ModelMatrix[0], new vec3(_modelScaleFactor));
                //    ModelMatrix[0] = glm.rotate(ModelMatrix[0], D2R(rotation), new vec3(1f, 0f, 0f));
                //    ModelMatrix[0] = glm.translate(ModelMatrix[0], -1 * ObjModel.Centroid);
                //}
                if (!MainVM.RotateWorld)
                {
                    Attitude att = null;

                    //   ViewMatrix[0] = glm.lookAt(new vec3(0.0f, 0.0f, ObjModel.Centroid.z * _cameraZxZoomFactor), new vec3(0.0f, 0.0f, 0.0f), new vec3(0.0f, 1.0f, 0.0f));

                    if (EWBBoard != null)
                    {
                        att = EWBBoard.GetAverageAttitude();
                        Acceleration[] acc = EWBBoard.GetAcceleration();
                        if(acc != null){
                            vec3[] data = new vec3[acc.Length];
                            for(int i=0; i < acc.Length; i++){
                                data[i] = acc[i].ToVec3();
                            }
                            RawAccPlot.FillBuffer(data);
                        }
                        MagneticField[] mag = EWBBoard.GetCompass();
                        if (mag != null)
                        {
                            vec3[] data = new vec3[acc.Length];
                            for (int i = 0; i < mag.Length; i++)
                            {
                                data[i] = mag[i].ToVec3();
                            }
                            RawMagPlot.FillBuffer(data);
                        }
                        Rotation[] rot = EWBBoard.GetGyro();
                        if (rot != null)
                        {
                            vec3[] data = new vec3[acc.Length];
                            for (int i = 0; i < rot.Length; i++)
                            {
                                data[i] = rot[i].ToVec3();
                            }
                            RawGyroPlot.FillBuffer(data);
                        }
                        
                    }

                    if (att != null)
                    {
                        Debug.WriteLine("Attitude: X = {0}, Y = {1}, Head = {2}", att.angleX, att.angleY, att.heading);
                        _compassRot = att.heading;
                        if (_staticActivity != null)
                        {
                            if (_staticActivity.Process(att))
                                _staticActivity.Stop();
                        }

                        if (_dynamicActivity != null)
                        {
                            if (_dynamicActivity.Process(att))
                                _dynamicActivity.Stop(); 
                        }
                        // Conceptually operations need to be performed in the order - Scale, Rotate and Translate
                        // However the order of matrix multiplication is reversed - so matrices must be multiplied in the sequence - Translate, Rotate and Scale
                        // to get Scale, Rotate and Translate effect on the actual data.

                        ModelMatrix[0] = mat4.identity();
                        //ModelMatrix[0] = glm.translate(ModelMatrix[0], new vec3(0f, -5f, 0f));
                        //  Create a model matrix to make the model a little bigger.
                        ModelMatrix[0] = glm.scale(ModelMatrix[0], new vec3(_modelScaleFactor));
                        // Pitch = rotate about Z axes
                        ModelMatrix[0] = glm.rotate(ModelMatrix[0], D2R(att.angleX), new vec3(0f, 0f, 1f));
                        // Roll = rotate about X axes
                        ModelMatrix[0] = glm.rotate(ModelMatrix[0], D2R(att.angleY), new vec3(1f, 0f, 0f));
                        // Yaw = rotate about Y axis
                        ModelMatrix[0] = glm.rotate(ModelMatrix[0], D2R(att.heading), new vec3(0f, 1f, 0f));
                        ModelMatrix[0] = glm.translate(ModelMatrix[0], -1 * ObjModel.Centroid);


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
            normalMatrix = myGLM.transpose(glm.inverse(ModelMatrix[0]));
            //normalMatrix = glm.inverse(modelMatrix);
            // normalMatrix = glm.scale(modelMatrix, new vec3(1f)); ;

        }

        void DrawCompass(OpenGL GL)
        {
            mat4 model = mat4.identity();
            model = glm.translate(model, new vec3(7, -3, -6));
            //model = glm.scale(model,new vec3(0.2f));
            textureShader.Bind(GL);
            textureShader.SetUniform(GL, "projectionMatrix", glm.perspective(D2R(60), (float)cWidth / (float)cHeight, 0.01f, 100.0f));
            textureShader.SetUniform(GL, "viewMatrix", mat4.identity()); // glm.lookAt(new vec3(0f, 0f, -6f), new vec3(0f, 0f, 0f), new vec3(0.0f, 1.0f, 0.0f))
            textureShader.SetUniform(GL, "modelMatrix", model);
            textureShader.SetUniform(GL, "normalMatrix", myGLM.transpose(glm.inverse(model)));

            textureShader.SetUniform(GL, "sunLight.vColor", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.Ka", new vec3(.1f, .1f, .1f));
            textureShader.SetUniform(GL, "sunLight.Kd", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.vDirection", new vec3(0f, 0f, 1f));
            textureShader.SetUniform(GL, "specLight.vDirection", new vec3(0f, 0f, 0f));
            textureShader.SetUniform(GL, "specLight.Ks", new vec3(.9f, .9f, .9f));
            textureShader.SetUniform(GL, "specLight.Shininess", 100f);

            _compass.Render(GL, textureShader);

            model = mat4.identity();
            model = glm.translate(model, new vec3(7, -3, -6));
            model = glm.rotate(model, D2R(-1 * _compassRot), new vec3(0f, 0f, 1f));
            model = glm.scale(model, new vec3(.1f, .8f, 1f));

            textureShader.SetUniform(GL, "projectionMatrix", glm.perspective(D2R(60), (float)cWidth / (float)cHeight, 0.01f, 100.0f));
            textureShader.SetUniform(GL, "viewMatrix", mat4.identity()); // glm.lookAt(new vec3(0f, 0f, -6f), new vec3(0f, 0f, 0f), new vec3(0.0f, 1.0f, 0.0f))
            textureShader.SetUniform(GL, "modelMatrix", model);
            textureShader.SetUniform(GL, "normalMatrix", myGLM.transpose(glm.inverse(model)));
            _cArrow.Render(GL, textureShader);

            textureShader.Unbind(GL);

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

        void DrawPlot(OpenGL GL)
        {
            simpleShader.Bind(GL);
            bool[] acc = new bool[] { MainVM.AccXDisp, MainVM.AccYDisp, MainVM.AccZDisp };
            bool[] mag = new bool[] { MainVM.MagXDisp, MainVM.MagYDisp, MainVM.MagZDisp };
            bool[] gyr = new bool[] { MainVM.RollDisp, MainVM.PitchDisp, MainVM.YawDisp};
            RawAccPlot.Render(GL, simpleShader, acc);
            RawMagPlot.Render(GL, simpleShader, mag);
            RawGyroPlot.Render(GL, simpleShader, gyr);
            simpleShader.Unbind(GL);
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
            textureShader.Bind(GL);
            textureShader.SetUniform(GL, "projectionMatrix", projectionMatrix);
            textureShader.SetUniform(GL, "viewMatrix", ViewMatrix[0]);
            textureShader.SetUniform(GL, "modelMatrix", ModelMatrix[0]);
            textureShader.SetUniform(GL, "normalMatrix", normalMatrix);
            textureShader.SetUniform(GL, "sunLight.vColor", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.Ka", new vec3(.1f, .1f, .1f));
            textureShader.SetUniform(GL, "sunLight.Kd", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.vDirection", new vec3(0f, 0f, 1f));
            textureShader.SetUniform(GL, "specLight.vDirection", new vec3(0f, 0f, 1f));
            textureShader.SetUniform(GL, "specLight.Ks", new vec3(.9f, .9f, .9f));
            textureShader.SetUniform(GL, "specLight.Shininess", 50f);

            // GL.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            ObjModel.RenderObj(GL, textureShader);
            //GL.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            //modelMatrix = glm.translate(new mat4(1.0f), new vec3(0.0f, 5.0f, -1.0f));
            //shaderProgram.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());
            //ObjModel.RenderObj(GL);
            textureShader.Unbind(GL);
        }

        float theta = -180;
        float phi = 0;
        void DrawSkyBox(OpenGL GL)
        {
            //SetMatrices(0);
            viewMatrix = mat4.identity();
            projectionMatrix = mat4.identity();
            projectionMatrix = glm.perspective(D2R(60), (float)cWidth / (float)cHeight, 0.01f, 100.0f);
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
                    _compassRot = att.heading;
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

            textureShader.Bind(GL);
            textureShader.SetUniform(GL, "projectionMatrix", projectionMatrix);
            textureShader.SetUniform(GL, "viewMatrix", ViewMatrix[1]);
            textureShader.SetUniform(GL, "modelMatrix", ModelMatrix[1]);
            textureShader.SetUniform(GL, "normalMatrix", normalMatrix);
            textureShader.SetUniform(GL, "sunLight.vColor", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.Ka", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.Kd", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "sunLight.vDirection", new vec3(0f, 0f, 1f));
            textureShader.SetUniform(GL, "specLight.vDirection", new vec3(0f, 0f, 0f)); // disable specular light for the skybox
            textureShader.SetUniform(GL, "specLight.Ks", new vec3(1f, 1f, 1f));
            textureShader.SetUniform(GL, "specLight.Shininess", 50f);

            //textureShader.SetUniform(GL, "sunLight.vColor", new vec3(1f, 1f, 1f));
            //textureShader.SetUniform(GL, "sunLight.Ka", new vec3(.8f, .8f, .8f));
            //textureShader.SetUniform(GL, "sunLight.Kd", new vec3(1f, 1f, 1f));
            //textureShader.SetUniform(GL, "sunLight.vDirection", new vec3(0f, 0f, -1f));
            //textureShader.SetUniform(GL, "specLight.vDirection", new vec3(0f, 0f, 0f)); 

            skyBox.renderSkybox(GL, textureShader);
            textureShader.Unbind(GL);

        }


        /// <summary>
        /// Handles the Resized event of the openGLControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="SharpGL.SceneGraph.OpenGLEventArgs"/> instance containing the event data.</param>
        private void openGLControl_Resized(object sender, OpenGLEventArgs args)
        {
            //  TODO: Set the projection matrix here.

            cWidth = openGLControl.ActualWidth;
            cHeight = openGLControl.ActualHeight;
            Debug.WriteIf(_debug, "Width = " + cWidth.ToString() + " Height = " + cHeight.ToString());
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
            if (_simulate || EWBBoard.Open(cmbxPorts.Text))
            {
                MainVM.StartStopIsEnabled = true;
                Debug.WriteLine("Opened port to :" + cmbxPorts.Text);
                Properties.Settings.Default.SelectedPort = cmbxPorts.SelectedIndex;

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
                MainVM.ActivityIsEnabled = true;
            }
            else
            {
                _enableStop = false;
                btnStartStop.Content = "Start";
                EWBBoard.Stop();
            }

        }


        private void StartCountdown(FrameworkElement target)
        {
            var countdownAnimation = new StringAnimationUsingKeyFrames();

            for (var i = 5; i > 0; i--)
            {
                var keyTime = TimeSpan.FromSeconds(5 - i);
                var frame = new DiscreteStringKeyFrame(i.ToString(), KeyTime.FromTimeSpan(keyTime));
                countdownAnimation.KeyFrames.Add(frame);
            }
            countdownAnimation.KeyFrames.Add(new DiscreteStringKeyFrame(" ", KeyTime.FromTimeSpan(TimeSpan.FromSeconds(6))));
            Storyboard.SetTargetName(countdownAnimation, target.Name);
            Storyboard.SetTargetProperty(countdownAnimation, new PropertyPath(TextBlock.TextProperty));

            var countdownStoryboard = new Storyboard();
            countdownStoryboard.Children.Add(countdownAnimation);
            countdownStoryboard.Completed += CountdownTimer_Completed;
            countdownStoryboard.Begin(this);
        }

        private void CountdownTimer_Completed(object sender, EventArgs e)
        {
            MessageBox.Show("Time's up!");
        }

        /// <summary>
        /// Do stuff depending on the selected activity
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartActivity_Click(object sender, RoutedEventArgs e)
        {
            //StartCountdown(CountdownDisplay);
            _activityTimer = new ActivityTimer();
            _activityTimer.NotifyPerSec = () =>
            {
                MainVM.TimeElapsed = _activityTimer.GetElapsedTime().ToString();
            };
            _activityTimer.NotifyOnCompletion = () =>
            {
                //MessageBox.Show("Time's up!");
                
                // Start activity
                if (MainVM.DynamicActivity)
                {
                    MainVM.DisplayMessage = "Rotate along Roll axis!!";
                    _staticActivity = null;
                    _dynamicActivity = new DynamicActivity();
                    _dynamicActivity.NotifyPerSec = () =>
                    {
                        MainVM.TimeElapsed = _dynamicActivity.ElapsedTime.ToString() + "\n"  + _dynamicActivity.CurrentRPM.ToString();
                    };
                    _dynamicActivity.Start();
                }
                else
                {
                    MainVM.DisplayMessage = "Hold Still!!";
                    _staticActivity = new StaticActivity();
                    _staticActivity.SetReference(EWBBoard.CurrentAttitude.angleX, EWBBoard.CurrentAttitude.angleY, EWBBoard.CurrentAttitude.heading);
                    _staticActivity.NotifyPerSec = () =>
                    {
                        MainVM.TimeElapsed = _staticActivity.ElapsedTime.ToString();
                    };
                    _staticActivity.Start();
                    _dynamicActivity = null;
                }

            };
            MainVM.DisplayMessage = "Starting in ";
            _activityTimer.Start(true, new TimeSpan(0, 0, 5));
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Debug.WriteLine("Application EXIT Triggered..");
            Properties.Settings.Default.Save(); // persist all modified settings

            if (EWBBoard != null)
                EWBBoard.DoOnQuit();

            ObjModel.Release(openGLControl.OpenGL);
            skyBox.Release(openGLControl.OpenGL);
            _compass.Release(openGLControl.OpenGL);
            _cArrow.Release(openGLControl.OpenGL);
            RawAccPlot.Release(openGLControl.OpenGL);

            if (textureShader != null)
                textureShader.Delete(openGLControl.OpenGL);

            System.Environment.Exit(0);

            //if (_serialPortAdapter != null)
            //{
            //    _serialPortAdapter.Close();
            //}
            //// release OpenGL objects
            //if (_vao != null)
            //{
            //    for (int i = 0; i < _vao.Length; i++)
            //    {
            //        if (_vao[i] != null)
            //            _vao[i].Delete(openGLControl.OpenGL);
            //    }
            //}
            //if (shaderProgram != null)
            //    shaderProgram.Delete(openGLControl.OpenGL);

            //System.Environment.Exit(0);
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

        bool _debug = true;
        float _sensitivity = 0.25f;
        private void MouseWheelHandler(object sender, MouseWheelEventArgs args)
        {
            if (args.Delta > 0) // Delta is +ve when the mouse wheel is rotated away from user (zoom-out)
            {
                Debug.WriteIf(_debug, "+ve");
                _cameraZxZoomFactor -= _sensitivity;
            }
            else
            {
                Debug.WriteIf(_debug, "-ve"); // Delta is +ve when the mouse wheel is rotated towards user (zoom-in)
                _cameraZxZoomFactor += _sensitivity;
            }
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
            textureShader.Bind(GL);
            textureShader.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "viewMatrix", viewMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "normalMatrix", normalMatrix.to_array());

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
            textureShader.Unbind(GL);

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
            textureShader.Bind(GL);
            //shaderProgram.SetUniformMatrix4(gl, "projectionMatrix", projectionMatrix.to_array());
            //shaderProgram.SetUniformMatrix4(gl, "viewMatrix", viewMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "modelview", modelMatrix.to_array());

            //  Bind the out vertex array.
            vertexBufferArray.Bind(GL);

            GL.DrawArrays(OpenGL.GL_LINES, 0, 8);

            //  Unbind our vertex array and shader.
            vertexBufferArray.Unbind(GL);
            textureShader.Unbind(GL);

            // --------------- Implementation WITH VERTEX ARRAY OBJECTS END --------------------

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
            textureShader.Bind(GL);
            textureShader.SetUniformMatrix4(GL, "projectionMatrix", projectionMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "viewMatrix", viewMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "modelMatrix", modelMatrix.to_array());
            textureShader.SetUniformMatrix4(GL, "normalMatrix", normalMatrix.to_array());

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
            textureShader.Unbind(GL);
        }

        Texture _texture;



    }




}
