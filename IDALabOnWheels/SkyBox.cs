using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL.SceneGraph;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using SharpGL.SceneGraph;
using System.Diagnostics;
using GlmNet;
using System.Windows.Threading;
using DR = System.Drawing;
using SharpGL.SceneGraph.Primitives;
using System.IO;
using SharpGL.SceneGraph.Assets;
using System.Drawing;
using SGL = SharpGL.SceneGraph.Primitives;
using Assimp;
using Assimp.Configs;

namespace IDALabOnWheels
{
    class SkyBox
    {

        VertexBufferArray skyVAO;
        string[] TexPath;

        float C_BOX_END = 500f;

        // string a_sFront, string a_sBack, string a_sLeft, string a_sRight, string a_sTop, string a_sBottom
        public void loadSkybox(OpenGL GL, string[] texPath, GShaderProgram shader)
        {

            // default texture:
            TexPath = new string[6];
            Debug.Write(AppDomain.CurrentDomain.BaseDirectory);
            TexPath[0] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajlands1\\jajlands1_ft.jpg";
            TexPath[1] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajlands1\\jajlands1_bk.jpg";
            TexPath[2] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajlands1\\jajlands1_lf.jpg";
            TexPath[3] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajlands1\\jajlands1_rt.jpg";
            TexPath[4] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajlands1\\jajlands1_up.jpg";
            TexPath[5] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajlands1\\jajlands1_dn.jpg";
            //TexPath[0] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\field\\front.jpg";
            //TexPath[1] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\field\\back.jpg";
            //TexPath[2] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\field\\right.jpg";
            //TexPath[3] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\field\\left.jpg";
            //TexPath[4] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\field\\up.jpg";
            //TexPath[5] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\field\\down.jpg";

            //TexPath[0] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajsundown\\jajsundown1_ft.jpg";
            //TexPath[1] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajsundown\\jajsundown1_bk.jpg";
            //TexPath[2] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajsundown\\jajsundown1_lf.jpg";
            //TexPath[3] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajsundown\\jajsundown1_rt.jpg";
            //TexPath[4] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajsundown\\jajsundown1_up.jpg";
            //TexPath[5] = AppDomain.CurrentDomain.BaseDirectory + "\\skyboxes\\jajsundown\\jajsundown1_dn.jpg";

            for (int i = 0; i < 6; i++)
            {
                TextureManager.Instance.CreateTexture(TexPath[i], GL);
                GL.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
                GL.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_EDGE);
                GL.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_EDGE);
            }
            // 24 vertices in a cube
            vec3[] vSkyBoxVertices = new vec3[]{
		// Front face
		 new vec3(C_BOX_END, C_BOX_END, C_BOX_END),  new vec3(C_BOX_END, -C_BOX_END, C_BOX_END),  new vec3(-C_BOX_END, C_BOX_END, C_BOX_END),  new vec3(-C_BOX_END, -C_BOX_END, C_BOX_END),
		// Back face
		 new vec3(-C_BOX_END, C_BOX_END, -C_BOX_END),  new vec3(-C_BOX_END, -C_BOX_END, -C_BOX_END),  new vec3(C_BOX_END, C_BOX_END, -C_BOX_END),  new vec3(C_BOX_END, -C_BOX_END, -C_BOX_END),
		// Left face
		 new vec3(-C_BOX_END, C_BOX_END, C_BOX_END),  new vec3(-C_BOX_END, -C_BOX_END, C_BOX_END),  new vec3(-C_BOX_END, C_BOX_END, -C_BOX_END),  new vec3(-C_BOX_END, -C_BOX_END, -C_BOX_END),
		// Right face
		 new vec3(C_BOX_END, C_BOX_END, -C_BOX_END),  new vec3(C_BOX_END, -C_BOX_END, -C_BOX_END),  new vec3(C_BOX_END, C_BOX_END, C_BOX_END),  new vec3(C_BOX_END, -C_BOX_END, C_BOX_END),
		// Top face
		 new vec3(-C_BOX_END, C_BOX_END, -C_BOX_END),  new vec3(C_BOX_END, C_BOX_END, -C_BOX_END),  new vec3(-C_BOX_END, C_BOX_END, C_BOX_END),  new vec3(C_BOX_END, C_BOX_END, C_BOX_END),
		// Bottom face
		 new vec3(C_BOX_END, -C_BOX_END, -C_BOX_END),  new vec3(-C_BOX_END, -C_BOX_END, -C_BOX_END),  new vec3(C_BOX_END, -C_BOX_END, C_BOX_END),  new vec3(-C_BOX_END, -C_BOX_END, C_BOX_END),
	};
            // each vertex should have a texture coordinate
            vec2[] SkyBoxTexCoords = new vec2[]{
		new vec2(0.0f, 1.0f), new vec2(0.0f, 0.0f), new vec2(1.0f, 1.0f), new vec2(1.0f, 0.0f)
	};


            vec3[] SkyBoxNormals = new vec3[]{
		 new vec3(0.0f, 0.0f, -1.0f),
		 new vec3(0.0f, 0.0f, 1.0f),
		 new vec3(1.0f, 0.0f, 0.0f),
		 new vec3(-1.0f, 0.0f, 0.0f),
		 new vec3(0.0f, -1.0f, 0.0f),
		 new vec3(0.0f, 1.0f, 0.0f)
	};

            vec2[] vSkyBoxTexCoords = new vec2[vSkyBoxVertices.Length];
            vec3[] vSkyBoxNormals = new vec3[vSkyBoxVertices.Length];
            vec3[] vSkyBoxColor = new vec3[vSkyBoxVertices.Length];

            // Build the data using a bit of magic
            for (int i = 0; i < 24; i++)
            {
                vSkyBoxTexCoords[i] = SkyBoxTexCoords[i % 4];
                vSkyBoxNormals[i] = SkyBoxNormals[i / 4];
                vSkyBoxColor[i] = GL.Color(Color.White);
            }

            skyVAO = new VertexBufferArray();
            skyVAO.Create(GL);
            skyVAO.Bind(GL);

            VertexBuffer[] skyVBO = new VertexBuffer[3];

            skyVBO[0] = new VertexBuffer();
            skyVBO[0].Create(GL);
            skyVBO[0].Bind(GL);
            skyVBO[0].SetData(GL, (uint)shader.GetAttributeID(GL, "vPosition"), vSkyBoxVertices, false, 3);

            //  Texture
            skyVBO[1] = new VertexBuffer();
            skyVBO[1].Create(GL);
            skyVBO[1].Bind(GL);
            skyVBO[1].SetData(GL, (uint)shader.GetAttributeID(GL, "vTextureCoord"), vSkyBoxTexCoords, false, 2);

            //  Normals
            skyVBO[2] = new VertexBuffer();
            skyVBO[2].Create(GL);
            skyVBO[2].Bind(GL);
            skyVBO[2].SetData(GL, (uint)shader.GetAttributeID(GL, "vSurfaceNormal"), vSkyBoxNormals, false, 3);

            //skyVBO[3] = new VertexBuffer();
            //skyVBO[3].Create(GL);
            //skyVBO[3].Bind(GL);
            //skyVBO[3].SetData(GL, VertexAttributes.Instance.AttrbColor, vSkyBoxColor, false, 3);

            skyVAO.Unbind(GL);


        }


        public void renderSkybox(OpenGL GL, GShaderProgram shader)
        {

            GL.DepthMask(0); // Clear depth checking so that the skybox is not clipped out by some other figure
            skyVAO.Bind(GL);
            for (int i = 0; i < 6; i++)
            {
                TexContainer tc = TextureManager.Instance.GetElement(TexPath[i]);
                tc.Tex.Bind(GL); // Bind to the current texture on texture unit 0
                GL.ActiveTexture(OpenGL.GL_TEXTURE0 + (uint)tc.ID);
                shader.SetUniform(GL, "uSampler", (int)tc.ID);
                //GL.Uniform1(Uniforms.Instance.Sampler, (int)tc.ID);

                GL.DrawArrays(OpenGL.GL_TRIANGLE_STRIP, i * 4, 4);
            }
            skyVAO.Unbind(GL);
            GL.DepthMask(1);
        }

        public void Release(OpenGL GL)
        {
            for (int i = 0; i < 6; i++)
            {
                TextureManager.Instance.Release(GL,TexPath[i]);
            }
            skyVAO.Delete(GL);

        }
    }
}
