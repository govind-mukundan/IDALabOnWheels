using GlmNet;
using SharpGL;
using SharpGL.VertexBuffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{

class Particle                                                                        
{

   vec3 vPosition;
   vec3 vVelocity;
   vec3 vColor;
   float fLifeTime;
   float fSize;
   int iType; // Regulat particle or Generator particle
};

    /// <summary>
    ///  Class to manage the creation, update and rendering of particle systems.
    ///  
    /// A particle system using transform feedback needs two sets of shaders. The first vertex and geometry shader simply 
    /// generate and update the particles. The next set of vertex, geometry and fragment shaders actually renders them
    /// </summary>

    class ParticleSystem
    {
        GShaderProgram spUpdate;
        GShaderProgram spRender;
        string[] vertexShaderSource;
        string[] fragmentShaderSource;
        string[] geomShaderSource;
        uint[] tFeedbackObj;
        uint[] query;
        VertexBufferArray[] particleVAO; // We use double buffering for the particle system

        int MAX_PARTICLES_ON_SCENE = 100000;

        public void Init(OpenGL gl)
        {
            spUpdate = new GShaderProgram();
            spRender = new GShaderProgram();
            vertexShaderSource = new string[2];
            fragmentShaderSource = new string[2];
            geomShaderSource = new string[2];
            tFeedbackObj = new uint[1];
            query = new uint[1];
            particleVAO = new VertexBufferArray[2];
            string[] sVaryings = {
		"vPositionOut",
		"vVelocityOut",
		"vColorOut",
		"fLifeTimeOut",
		"fSizeOut",
		"iTypeOut"};

            vertexShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\particleGen.vert");
            geomShaderSource[0] = ManifestResourceLoader.LoadTextFile("Shaders\\particleGen.geom");
            fragmentShaderSource[0] = null;
            vertexShaderSource[1] = ManifestResourceLoader.LoadTextFile("Shaders\\particleRender.vert");
            fragmentShaderSource[1] = ManifestResourceLoader.LoadTextFile("Shaders\\particleRender.frag");
            geomShaderSource[1] = ManifestResourceLoader.LoadTextFile("Shaders\\particleGenRender.geom");
            spUpdate.Create(gl, vertexShaderSource[0], fragmentShaderSource[0], geomShaderSource[0], sVaryings);
            spRender.Create(gl, vertexShaderSource[1], fragmentShaderSource[1], geomShaderSource[1], null);
            spUpdate.AssertValid(gl);
            spRender.AssertValid(gl);

            gl.GenTransformFeedbacks(1, tFeedbackObj);
            gl.GenQueries(1, query);

            for (int i = 0; i < particleVAO.Length; i++)
            {
                particleVAO[i].Create(gl);
                particleVAO[i].Bind(gl);

                VertexBuffer[] objVBO = new VertexBuffer[3];
                objVBO[i] = new VertexBuffer();
                objVBO[i].Create(gl);
                objVBO[i].Bind(gl);
                gl.BufferData(OpenGL.GL_ARRAY_BUFFER, Marshal.SizeOf(typeof(Particle)) * MAX_PARTICLES_ON_SCENE,(IntPtr)null, OpenGL.GL_DYNAMIC_DRAW);
                gl.BufferSubData(OpenGL.GL_ARRAY_BUFFER, 0, Marshal.SizeOf(typeof(Particle)), &partInitialization);
            }
                //  Generate the vertex array.

                gl.GenBuffers(1, ids);
            vertexBufferObject = ids[0];
   //glGenQueries(1, &uiQuery);

   //glGenBuffers(2, uiParticleBuffer);
   //glGenVertexArrays(2, uiVAO);

   //CParticle partInitialization;
   //partInitialization.iType = PARTICLE_TYPE_GENERATOR;

   //FOR(i, 2)
   //{   
   //   glBindVertexArray(uiVAO[i]);
   //   glBindBuffer(GL_ARRAY_BUFFER, uiParticleBuffer[i]);
   //   glBufferData(GL_ARRAY_BUFFER, sizeof(CParticle)*MAX_PARTICLES_ON_SCENE, NULL, GL_DYNAMIC_DRAW);
   //   glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(CParticle), &partInitialization);

   //   FOR(i, NUM_PARTICLE_ATTRIBUTES)glEnableVertexAttribArray(i);

   //   glVertexAttribPointer(0, 3, GL_FLOAT, GL_FALSE, sizeof(CParticle), (const GLvoid*)0); // Position
   //   glVertexAttribPointer(1, 3, GL_FLOAT, GL_FALSE, sizeof(CParticle), (const GLvoid*)12); // Velocity
   //   glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, sizeof(CParticle), (const GLvoid*)24); // Color
   //   glVertexAttribPointer(3, 1, GL_FLOAT, GL_FALSE, sizeof(CParticle), (const GLvoid*)36); // Lifetime
   //   glVertexAttribPointer(4, 1, GL_FLOAT, GL_FALSE, sizeof(CParticle), (const GLvoid*)40); // Size
   //   glVertexAttribPointer(5, 1, GL_INT,     GL_FALSE, sizeof(CParticle), (const GLvoid*)44); // Type
   //}
   //iCurReadBuffer = 0;
   //iNumParticles = 1;

   //bInitialized = true;

            
        }
    }
}
