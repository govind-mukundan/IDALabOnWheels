using GlmNet;
using SharpGL;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class Compass
    {
        VertexBufferArray compassVAO;
        string C_DUMMY_TEXTURE = "DummyCompass1x1"; // A dummy texture path for 1x1 textures that are created from colors
        int _numSegments = 100;
        string _textureName;

        class CircleData
        {
            public vec3[] points;
            public vec2[] texture;
        }

        CircleData DrawCircle(float cx, float cy, float r, int num_segments)
        {
            CircleData c = new CircleData();
            vec3[] circle_v = new vec3[num_segments];
            vec2[] cTex = new vec2[num_segments];
            for (int ii = 0; ii < num_segments; ii++)
            {
                float theta = 2.0f * 3.1415926f * ii / num_segments;//get the current angle 
                circle_v[ii].x = r * (float)Math.Cos(theta);//calculate the x component 
                circle_v[ii].y = r * (float)Math.Sin(theta);//calculate the y component 

                // Texture coordinates - (0,0) on circle must map to (0.5,0.5) texture and (1,1) maps to (1,1)..
                cTex[ii].x = (float)Math.Cos(theta) * 0.5f + 0.5f;
                cTex[ii].y = (float)Math.Sin(theta) * 0.5f + 0.5f;
            }
            c.points = circle_v;
            c.texture = cTex;
            return (c);
        }

        public void Create(OpenGL GL, string texPath, GShaderProgram shader)
        {
            CircleData c = DrawCircle(0f, 0f, 1f, _numSegments);
            vec3[] points = c.points;
            vec3[] normals = new vec3[points.Length];
            vec2[] texCord = c.texture;
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new vec3(0.0f, 0.0f, -1.0f);
            }


            if (texPath == null)
            {
                _textureName = C_DUMMY_TEXTURE;
                TextureManager.Instance.CreateTexture1x1(C_DUMMY_TEXTURE, GL, false, Color.Blue);
            }
            else
            {
                _textureName = texPath;
                TextureManager.Instance.CreateTexture(texPath, GL, false);
            }

            compassVAO = new VertexBufferArray();
            compassVAO.Create(GL);
            compassVAO.Bind(GL);

            VertexBuffer[] cVBO = new VertexBuffer[3];

            cVBO[0] = new VertexBuffer();
            cVBO[0].Create(GL);
            cVBO[0].Bind(GL);
            cVBO[0].SetData(GL, (uint)shader.GetAttributeID(GL, "vPosition"), points, false, 3);

            //  Texture
            cVBO[1] = new VertexBuffer();
            cVBO[1].Create(GL);
            cVBO[1].Bind(GL);
            cVBO[1].SetData(GL, (uint)shader.GetAttributeID(GL, "vTextureCoord"), texCord, false, 2);

            //  Normals
            cVBO[2] = new VertexBuffer();
            cVBO[2].Create(GL);
            cVBO[2].Bind(GL);
            cVBO[2].SetData(GL, (uint)shader.GetAttributeID(GL, "vSurfaceNormal"), normals, false, 3);

           compassVAO.Unbind(GL);
        }

        public void Render(OpenGL GL, GShaderProgram shader)
        {
            compassVAO.Bind(GL);
            TexContainer tc;
            tc = TextureManager.Instance.GetElement(_textureName);
            tc.Tex.Bind(GL); // Bind to the current texture on texture unit 0
            GL.ActiveTexture(OpenGL.GL_TEXTURE0 + (uint)tc.ID);
            shader.SetUniform(GL, "uSampler", tc.ID);
            // Turn ON aplha blending for the arrow so the empty background is ignored
            GL.Enable(OpenGL.GL_BLEND);
            GL.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            GL.DrawArrays(OpenGL.GL_TRIANGLE_FAN, 0, _numSegments);
            GL.Disable(OpenGL.GL_BLEND);

            compassVAO.Unbind(GL);

        }

        public void Release(OpenGL GL)
        {
            TextureManager.Instance.Release(GL,_textureName);
            compassVAO.Delete(GL);

        }
    }


    class CompassArrow
    {
        VertexBufferArray arrowVAO;
        string C_DUMMY_TEXTURE = "DummyArrow1x1"; // A dummy texture path for 1x1 textures that are created from colors
        string _textureName;
        float C_BOX_END = 1f;

        public void Create(OpenGL GL, string texPath, GShaderProgram shader)
        {
            vec3[] points = new vec3[] { /*new vec3(-C_END, -C_END, 0), new vec3(-C_END, C_END, 0), new vec3(C_END, C_END, 0), new vec3(C_END, C_END, 0) */
            
            new vec3(C_BOX_END, C_BOX_END, 0),  new vec3(C_BOX_END, -C_BOX_END, 0),  new vec3(-C_BOX_END, C_BOX_END, 0),  new vec3(-C_BOX_END, -C_BOX_END, 0)
        };
            vec3[] normals = new vec3[points.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = new vec3(0.0f, 0.0f, -1.0f);
            }
            vec2[] texCord = new vec2[]{
		new vec2(0.0f, 1.0f), new vec2(0.0f, 0.0f), new vec2(1.0f, 1.0f), new vec2(1.0f, 0.0f)
	};

            if (texPath == null)
            {
                _textureName = C_DUMMY_TEXTURE;
                TextureManager.Instance.CreateTexture1x1(C_DUMMY_TEXTURE, GL, false, Color.Blue);
            }
            else
            {
                _textureName = texPath;
                TextureManager.Instance.CreateTexture(texPath, GL, false);
            }

            arrowVAO = new VertexBufferArray();
            arrowVAO.Create(GL);
            arrowVAO.Bind(GL);

            VertexBuffer[] cVBO = new VertexBuffer[3];

            cVBO[0] = new VertexBuffer();
            cVBO[0].Create(GL);
            cVBO[0].Bind(GL);
            cVBO[0].SetData(GL, (uint)shader.GetAttributeID(GL, "vPosition"), points, false, 3);

            //  Texture
            cVBO[1] = new VertexBuffer();
            cVBO[1].Create(GL);
            cVBO[1].Bind(GL);
            cVBO[1].SetData(GL, (uint)shader.GetAttributeID(GL, "vTextureCoord"), texCord, false, 2);

            //  Normals
            cVBO[2] = new VertexBuffer();
            cVBO[2].Create(GL);
            cVBO[2].Bind(GL);
            cVBO[2].SetData(GL, (uint)shader.GetAttributeID(GL, "vSurfaceNormal"), normals, false, 3);

            arrowVAO.Unbind(GL);
        }

        public void Render(OpenGL GL, GShaderProgram shader)
        {
            arrowVAO.Bind(GL);
            TexContainer tc;
            tc = TextureManager.Instance.GetElement(_textureName);
            tc.Tex.Bind(GL); // Bind to the current texture on texture unit 0
            GL.ActiveTexture(OpenGL.GL_TEXTURE0 + (uint)tc.ID);
            shader.SetUniform(GL, "uSampler", tc.ID);

            // Turn ON aplha blending for the arrow so the empty background is ignored
            GL.Enable(OpenGL.GL_BLEND);
            GL.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            GL.DrawArrays(OpenGL.GL_TRIANGLE_STRIP, 0, 4);
            GL.Disable(OpenGL.GL_BLEND);

            arrowVAO.Unbind(GL);

        }

        public void Release(OpenGL GL){
                        TextureManager.Instance.Release(GL,_textureName);
                        arrowVAO.Delete(GL);
        }
    }
}
