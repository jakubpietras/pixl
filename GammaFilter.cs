using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;

namespace Pixl
{
    internal class GammaFilter : IFilter
    {
        public string Name {  get; set; }
        public float Gamma { get; set; }
        public GammaFilter(string name, float gamma)
        {
            Name = name;
            Gamma = gamma;
        }

        public void Apply(byte[] rgbValues, int width, int height, int bytes)
        {
            // byte[] rgbValuesTmp = new byte[bytes];
            
            for (int counter = 0; counter < rgbValues.Length; counter++)
            {
                double val = (double)rgbValues[counter] / 255;
                double newVal = Math.Pow(val, Gamma);
                rgbValues[counter] = (byte)(newVal * 255);
            }

            // rgbValuesTmp.CopyTo(rgbValues, 0);      
        }
    }
}
