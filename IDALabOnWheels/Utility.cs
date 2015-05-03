using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDALabOnWheels
{
    class Utility
    {

        public static float Median(float[] data)
        {
            if (data.Length == 0)
                return (0f);

            Array.Sort(data);

            if (data.Length == 0)
            {
                return (0f);
                throw new InvalidOperationException("Empty collection");
            }
            else if (data.Length % 2 == 0)
            {
                // count is even, average two middle elements
                float a = data[data.Length / 2 - 1];
                float b = data[data.Length / 2];
                return (float)Math.Round((a + b) / 2f);
            }
            else
            {
                // count is odd, return the middle element
                return data[data.Length / 2];
            }
        }

    }
}
