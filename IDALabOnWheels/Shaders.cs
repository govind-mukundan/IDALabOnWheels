using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class Shader
    {
                private static readonly Shader instance = new Shader();

                private Shader() { }

        public static Shader Instance
        {
            get
            {
                if (instance == null)
                {
                    return instance;
                }
                return instance;
            }
        }

        public uint AttrbColor;
        public uint AttrbPosition;
        public uint AttrbTexture;
        public uint AttrbSurfaceNormal;
    }
}
