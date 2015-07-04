using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DR = System.Drawing;
using GlmNet;
using SharpGL.VertexBuffers;
using SharpGL;
using System.Diagnostics;


namespace IDALabOnWheels
{
    class LinePlot
    {
        public  int C_MAX_LSB = 700;
        public  int C_MIN_LSB = -700;
        public vec2 C_MAX;
        public vec2 C_MIN;
        VertexBufferArray plotVAO;
        VertexBuffer[] plotVBO;
        vec3[][] _buffer;
        vec3[][] _dummyColors;
        vec3[] _margin;
        vec3[] _marginColor;
        int _bufferIndex;
        int C_BUFFER_SIZE = 256;
        string[] _textureID;
        bool _debug = true;
        int _axes;
        // Since the viewport doesnt actually occupy the whole screen, we scale it by a factor less than .5
        // Note that this is fixed and so will not maintain ration if the screen is resized
        float C_DISP_SCREEN = 0.5f;

        /// <summary>
        /// Bounds specify the bounds in device independent coordinates of the plot. 
        /// (Min-X, Min-Y), (Max-X, Max-Y)
        /// ID is a unique identifier for the particular line plot
        /// </summary>
        /// <param name="color"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="bounds"></param>
        public void Initialize(OpenGL GL, DR.Color[] color, int min, int max, string[] ID, int axes)
        {
            C_MAX_LSB = max;
            C_MIN_LSB = min;
            //C_MAX = bounds_max;
            //C_MIN = bounds_min;
            _axes = axes;
            _buffer = new vec3[axes][];
            _dummyColors = new vec3[axes][];
            _margin = new vec3[4];
            _marginColor = new vec3[4];
            plotVBO = new VertexBuffer[2];

            // vertices for margin
            _margin[0] = new vec3(-1, -1, 0);
            _margin[1] = new vec3(-1, 0, 0);
            _margin[2] = new vec3(0, 0, 0);
            _margin[3] = new vec3(0, -1, 0);
            for (int i = 0; i < _margin.Length; i++)
            {
                _marginColor[i] = SharpGLEx.GetColor(DR.Color.White);
            }

                for (int j = 0; j < axes; j++)
                {
                    _buffer[j] = new vec3[C_BUFFER_SIZE];
                    _dummyColors[j] = new vec3[C_BUFFER_SIZE];
                    // Initialize the X values range from -1 to -0.5
                    for (int i = 0; i < C_BUFFER_SIZE; i++)
                    {
                        _buffer[j][i].x = 1f * (i - 0) / (C_BUFFER_SIZE - 0) - 1;
                        _dummyColors[j][i] = SharpGLEx.GetColor(color[j]);
                    }
                }
            _bufferIndex = 0;

            _textureID = ID;
            for (int i = 0; i < _axes; i++)
            {
                TextureManager.Instance.CreateTexture1x1(_textureID[i], GL, false, color[i]);
            }

            plotVAO = new VertexBufferArray();
            plotVAO.Create(GL);
            plotVAO.Bind(GL);
            
            plotVBO[0] = new VertexBuffer();
            plotVBO[0].Create(GL);
            plotVBO[0].Bind(GL);

            plotVBO[1] = new VertexBuffer();
            plotVBO[1].Create(GL);
            plotVBO[1].Bind(GL);


            plotVAO.Unbind(GL);

        }

        public void FillBuffer(float[] data)
        {
            int len = 0;
            if (_bufferIndex + data.Length >= C_BUFFER_SIZE) // data is going to roll over?
                _bufferIndex = 0;

            // Normalize the data with respect to the range [-1,1] and then shift it into the bounds
            for (int i = 0; i < data.Length; i++)
            {
                _buffer[0][_bufferIndex++].y = 0.5f * (2.0f * (data[i] - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 1) - 0.5f;
            }

           // Debug.WriteLineIf(_debug, "AccZ = " + data[0].ToString());
        }

        /// <summary>
        /// Fill buffer with data from 3 axes
        /// </summary>
        /// <param name="data"></param>
        public void FillBuffer(vec3[] data)
        {
            try { 
            if (_bufferIndex + data.Length >= C_BUFFER_SIZE) // data is going to roll over?
                _bufferIndex = 0;

            // Normalize the data with respect to the range [-1,1] and then shift it into the bounds
            for (int i = 0; i < data.Length; i++)
            {
                _buffer[0][_bufferIndex].y =  (-1.0f * (data[i].x - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 0) ;
                _buffer[1][_bufferIndex].y =  (-1.0f * (data[i].y - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 0) ;
                _buffer[2][_bufferIndex].y = (-1.0f * (data[i].z - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 0) ;
                _bufferIndex++;
            }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public void Render(OpenGL GL,  GShaderProgram shader, bool[] enabled)
        {
            // don't draw margin if everything is OFF
            if (enabled[0] == false && enabled[1] == false && enabled[2] == false)
                return;

            plotVAO.Bind(GL);
            for (int i = 0; i < _axes; i++)
            {
                if (enabled[i] == false) continue;

                plotVBO[0].Bind(GL);
                plotVBO[0].SetData(GL, (uint)shader.GetAttributeID(GL, "vPosition"), _buffer[i], false, 3);
                plotVBO[1].Bind(GL);
                plotVBO[1].SetData(GL, (uint)shader.GetAttributeID(GL, "vColor"), _dummyColors[i], false, 3);
                GL.DrawArrays(OpenGL.GL_LINE_STRIP, 0, _bufferIndex);
            }

            plotVBO[0].Bind(GL);
            plotVBO[0].SetData(GL, (uint)shader.GetAttributeID(GL, "vPosition"), _margin, false, 3);
            plotVBO[1].Bind(GL);
            plotVBO[1].SetData(GL, (uint)shader.GetAttributeID(GL, "vColor"), _marginColor, false, 3);
            GL.LineStipple(1, 0x00FF);
            GL.Enable(OpenGL.GL_LINE_STIPPLE);
            GL.DrawArrays(OpenGL.GL_LINE_STRIP, 0, 4);// X - Minor Grid
            GL.Disable(OpenGL.GL_LINE_STIPPLE);
            plotVAO.Unbind(GL);
        }

        public void Release(OpenGL GL)
        {
            plotVBO[0].Unbind(GL);
            plotVBO[1].Unbind(GL);
            plotVAO.Unbind(GL);
            plotVAO.Delete(GL);
        }
    }
}
