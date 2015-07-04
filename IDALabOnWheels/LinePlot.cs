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
        int _bufferIndex;
        int C_BUFFER_SIZE = 512;
        string[] _textureID;
        bool _debug = true;
        int _axes;

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
            plotVBO = new VertexBuffer[2];

            for (int j = 0; j < axes; j++)
            {
                _buffer[j] = new vec3[C_BUFFER_SIZE];
                _dummyColors[j] = new vec3[C_BUFFER_SIZE];
                // Initialize the X values range from -1 to -0.5
                for (int i = 0; i < C_BUFFER_SIZE; i++)
                {
                    _buffer[j][i].x = 0.5f * (i - 0) / (C_BUFFER_SIZE - 0) - 1;
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
            if (_bufferIndex + data.Length >= C_BUFFER_SIZE) // data is going to roll over?
                _bufferIndex = 0;

            // Normalize the data with respect to the range [-1,1] and then shift it into the bounds
            for (int i = 0; i < data.Length; i++)
            {
                _buffer[0][_bufferIndex++].y = 0.5f * (2.0f * (data[i] - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 1) - 0.5f;
            }

           // Debug.WriteLineIf(_debug, "AccZ = " + data[0].ToString());
        }

        public void FillBuffer(vec3[] data)
        {
            if (_bufferIndex + data.Length >= C_BUFFER_SIZE) // data is going to roll over?
                _bufferIndex = 0;

            // Since the viewport doesnt actually occupy the whole screen, we scale it by a factor less than .5
            // Note that this is fixed and so will not maintain ration if the screen is resized
            float C_DISP_SCREEN = 0.4f;
            // Normalize the data with respect to the range [-1,1] and then shift it into the bounds
            for (int i = 0; i < data.Length; i++)
            {
                _buffer[0][_bufferIndex].y = C_DISP_SCREEN * (2.0f * (data[i].x - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 1) - C_DISP_SCREEN;
                _buffer[1][_bufferIndex].y = C_DISP_SCREEN * (2.0f * (data[i].y - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 1) - C_DISP_SCREEN;
                _buffer[2][_bufferIndex].y = C_DISP_SCREEN * (2.0f * (data[i].z - C_MIN_LSB) / (C_MAX_LSB - C_MIN_LSB) - 1) - C_DISP_SCREEN;
                _bufferIndex++;
            }

            // Debug.WriteLineIf(_debug, "AccZ = " + data[0].ToString());
        }

        public void Render(OpenGL GL,  GShaderProgram shader)
        {
            for (int i = 0; i < _axes; i++)
            {
                plotVAO.Bind(GL);
                plotVBO[0].Bind(GL);
                plotVBO[0].SetData(GL, (uint)shader.GetAttributeID(GL, "vPosition"), _buffer[i], false, 3);
                plotVBO[1].Bind(GL);
                plotVBO[1].SetData(GL, (uint)shader.GetAttributeID(GL, "vColor"), _dummyColors[i], false, 3);
                GL.DrawArrays(OpenGL.GL_LINE_STRIP, 0, _bufferIndex);
            }
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
