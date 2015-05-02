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

            float[] temp = new float[data.Length - 1];
            // Discard the first element 0RR
            Array.Copy(data, 1, temp, 0, data.Length - 1);
            // Create a copy of the input, and sort the copy
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                return (0f);
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                float a = temp[count / 2 - 1];
                float b = temp[count / 2];
                return (float)Math.Round((a + b) / 2f);
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }

    }
}
