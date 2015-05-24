using GlmNet;
using SharpGL;
using SharpGL.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    // Dictionary of KV pairs
    static class GLSLUniforms<T>
    {






    }

    class GLSLAttributes
    {

    }

    // Subclasses shader program to also store and manage Uniforms and Arrtibutes
    class GShaderProgram
    {

        private readonly Shader vertexShader = new Shader();
        private readonly Shader fragmentShader = new Shader();
        readonly Shader geometryShader = new Shader();


        public void Create(OpenGL gl, string vertexShaderSource, string fragmentShaderSource, string geomShaderSource, string[] transformFeedbackVaryings)
        {
            //  Create the program,
            shaderProgramObject = gl.CreateProgram();
            //  Create the shaders. attach the shaders.
            if (vertexShaderSource != null)
            {
                vertexShader.Create(gl, OpenGL.GL_VERTEX_SHADER, vertexShaderSource);
                gl.AttachShader(shaderProgramObject, vertexShader.ShaderObject);
            }
            if (fragmentShader != null) { 
                fragmentShader.Create(gl, OpenGL.GL_FRAGMENT_SHADER, fragmentShaderSource);
                gl.AttachShader(shaderProgramObject, fragmentShader.ShaderObject);
            }
            if (geometryShader != null) { 
                geometryShader.Create(gl, OpenGL.GL_GEOMETRY_SHADER, geomShaderSource);
                gl.AttachShader(shaderProgramObject, geometryShader.ShaderObject);
            }

            if (transformFeedbackVaryings != null)
            gl.TransformFeedbackVaryings(ShaderProgramObject, transformFeedbackVaryings.Length, transformFeedbackVaryings, OpenGL.GL_INTERLEAVED_ATTRIBS);
            //  Now we can link the program.
            gl.LinkProgram(shaderProgramObject);

            //  Now that we've compiled and linked the shader, check it's link status. If it's not linked properly, we're
            //  going to throw an exception.
            if (GetLinkStatus(gl) == false)
            {
                throw new ShaderCompilationException(string.Format("Failed to link shader program with ID {0}.", shaderProgramObject), GetInfoLog(gl));
            }
        }
        ///// <summary>
        ///// Creates the shader program.
        ///// </summary>
        ///// <param name="gl">The gl.</param>
        ///// <param name="vertexShaderSource">The vertex shader source.</param>
        ///// <param name="fragmentShaderSource">The fragment shader source.</param>
        ///// <param name="attributeLocations">The attribute locations. This is an optional array of
        ///// uint attribute locations to their names.</param>
        ///// <exception cref="ShaderCompilationException"></exception>
        //public void Create(OpenGL gl, string vertexShaderSource, string fragmentShaderSource,
        //    Dictionary<uint, string> attributeLocations)
        //{
        //    //  Create the shaders.
        //    vertexShader.Create(gl, OpenGL.GL_VERTEX_SHADER, vertexShaderSource);
        //    fragmentShader.Create(gl, OpenGL.GL_FRAGMENT_SHADER, fragmentShaderSource);

        //    //  Create the program, attach the shaders.
        //    shaderProgramObject = gl.CreateProgram();
        //    gl.AttachShader(shaderProgramObject, vertexShader.ShaderObject);
        //    gl.AttachShader(shaderProgramObject, fragmentShader.ShaderObject);

        //    //  Before we link, bind any vertex attribute locations.
        //    if (attributeLocations != null)
        //    {
        //        foreach (var vertexAttributeLocation in attributeLocations)
        //            gl.BindAttribLocation(shaderProgramObject, vertexAttributeLocation.Key, vertexAttributeLocation.Value);
        //    }

        //    //  Now we can link the program.
        //    gl.LinkProgram(shaderProgramObject);

        //    //  Now that we've compiled and linked the shader, check it's link status. If it's not linked properly, we're
        //    //  going to throw an exception.
        //    if (GetLinkStatus(gl) == false)
        //    {
        //        throw new ShaderCompilationException(string.Format("Failed to link shader program with ID {0}.", shaderProgramObject), GetInfoLog(gl));
        //    }
        //}

        public void Delete(OpenGL gl)
        {
            gl.DetachShader(shaderProgramObject, vertexShader.ShaderObject);
            gl.DetachShader(shaderProgramObject, fragmentShader.ShaderObject);
            vertexShader.Delete(gl);
            fragmentShader.Delete(gl);
            gl.DeleteProgram(shaderProgramObject);
            shaderProgramObject = 0;
        }

        public int GetAttributeLocation(OpenGL gl, string attributeName)
        {
            return gl.GetAttribLocation(shaderProgramObject, attributeName);
        }

        public void BindAttributeLocation(OpenGL gl, uint location, string attribute)
        {
            gl.BindAttribLocation(shaderProgramObject, location, attribute);
        }

        public void Bind(OpenGL gl)
        {
            gl.UseProgram(shaderProgramObject);
        }

        public void Unbind(OpenGL gl)
        {
            gl.UseProgram(0);
        }

        public bool GetLinkStatus(OpenGL gl)
        {
            int[] parameters = new int[] { 0 };
            gl.GetProgram(shaderProgramObject, OpenGL.GL_LINK_STATUS, parameters);
            return parameters[0] == OpenGL.GL_TRUE;
        }

        public string GetInfoLog(OpenGL gl)
        {
            //  Get the info log length.
            int[] infoLength = new int[] { 0 };
            gl.GetProgram(shaderProgramObject, OpenGL.GL_INFO_LOG_LENGTH, infoLength);
            int bufSize = infoLength[0];

            //  Get the compile info.
            StringBuilder il = new StringBuilder(bufSize);
            gl.GetProgramInfoLog(shaderProgramObject, bufSize, IntPtr.Zero, il);

            return il.ToString();
        }

        public void AssertValid(OpenGL gl)
        {
            if (vertexShader.GetCompileStatus(gl) == false)
                throw new Exception(vertexShader.GetInfoLog(gl));
            if (fragmentShader.GetCompileStatus(gl) == false)
                throw new Exception(fragmentShader.GetInfoLog(gl));
            if (GetLinkStatus(gl) == false)
                throw new Exception(GetInfoLog(gl));
        }

        public void SetUniform1(OpenGL gl, string uniformName, float v1)
        {
            gl.Uniform1(GetUniformLocation(gl, uniformName), v1);
        }

        public void SetUniform3(OpenGL gl, string uniformName, float v1, float v2, float v3)
        {
            gl.Uniform3(GetUniformLocation(gl, uniformName), v1, v2, v3);
        }

        public void SetUniformMatrix3(OpenGL gl, string uniformName, float[] m)
        {
            gl.UniformMatrix3(GetUniformLocation(gl, uniformName), 1, false, m);
        }

        public void SetUniformMatrix4(OpenGL gl, string uniformName, float[] m)
        {
            gl.UniformMatrix4(GetUniformLocation(gl, uniformName), 1, false, m);
        }

        public int GetUniformLocation(OpenGL gl, string uniformName)
        {
            //  If we don't have the uniform name in the dictionary, get it's 
            //  location and add it.
            if (uniformNamesToLocations.ContainsKey(uniformName) == false)
            {
                uniformNamesToLocations[uniformName] = gl.GetUniformLocation(shaderProgramObject, uniformName);
                //  TODO: if it's not found, we should probably throw an exception.
            }

            //  Return the uniform location.
            return uniformNamesToLocations[uniformName];
        }

        /// <summary>
        /// Gets the shader program object.
        /// </summary>
        /// <value>
        /// The shader program object.
        /// </value>
        public uint ShaderProgramObject
        {
            get { return shaderProgramObject; }
        }

        private uint shaderProgramObject;

        /// <summary>
        /// A mapping of uniform names to locations. This allows us to very easily specify 
        /// uniform data by name, quickly looking up the location first if needed.
        /// </summary>
        private readonly Dictionary<string, int> uniformNamesToLocations = new Dictionary<string, int>();

        // Holds a list of uniforms names and their handles
        Dictionary<string, int> Uniforms = new Dictionary<string, int>();
        Dictionary<string, int> Attributes = new Dictionary<string, int>();


        public int CreateUniform(OpenGL gl, string key)
        {
            if (Uniforms.ContainsKey(key))
                throw new Exception("Duplicate Uniform Specified");

            int value = gl.GetUniformLocation(this.ShaderProgramObject, key);
            Uniforms.Add(key, value);
            return (value);
        }

        public int CreateAttribute(OpenGL gl, string key)
        {
            if (Uniforms.ContainsKey(key))
                throw new Exception("Duplicate Attribute Specified");

            int value = gl.GetAttribLocation(this.ShaderProgramObject, key);
            Attributes.Add(key, value);
            return (value);
        }

        int GetUniformID(OpenGL gl, string key)
        {
            if (Uniforms.ContainsKey(key) == false)
                CreateUniform(gl, key);

            return (Uniforms[key]);
        }

        public int GetAttributeID(OpenGL gl, string key)
        {
            if (Attributes.ContainsKey(key) == false)
                CreateAttribute(gl, key);

            return (Attributes[key]);
        }

        // Uniforms can be of multiple types - mat, int, float..
        public bool SetUniform(OpenGL gl, string key, object value)
        {
            bool ret = false;

            // Find the ID of the uniform
            int id = GetUniformID(gl,key);
            // Find the type of the uniform
            if (value.GetType() == typeof(int))
            {
                gl.Uniform1(id, (int)Convert.ToInt32(value));
            }
            else if (value.GetType() == typeof(float))
            {
                gl.Uniform1(id, (float)Convert.ToDecimal(value));
            }
            else if (value.GetType() == typeof(vec3))
            {
                vec3 v = (vec3)Convert.ChangeType(value, typeof(vec3));
                gl.Uniform3(id, v.x, v.y, v.z);
            }
            else if (value.GetType() == typeof(vec4)) {
                vec4 v = (vec4)Convert.ChangeType(value, typeof(vec4));
                gl.Uniform4(id, v.x, v.y, v.z,v.w);
            }
            else if (value.GetType() == typeof(mat3))
            {
                mat3 m = (mat3)Convert.ChangeType(value, typeof(mat3));
                gl.UniformMatrix4(id, 1, false, m.to_array());
            }
            else if (value.GetType() == typeof(mat4))
            {
                mat4 m = (mat4)Convert.ChangeType(value, typeof(mat4));
                gl.UniformMatrix4(id, 1, false, m.to_array());
            }
            else
            {
                throw new Exception("Unimplemented Uniform Type");
            }


            return (ret);
        }

    }



}
