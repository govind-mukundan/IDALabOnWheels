using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    public class Uniforms
    {

                private static readonly Uniforms instance = new Uniforms();

                private Uniforms() { }

                public static Uniforms Instance
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

        public int Sampler;
        public int SunlightColor;
        public int SunlightAmbientIntensity;
        public int SunlightDirection;
    }
}
