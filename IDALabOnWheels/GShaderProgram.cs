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
    class GShaderProgram : ShaderProgram
    {
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
