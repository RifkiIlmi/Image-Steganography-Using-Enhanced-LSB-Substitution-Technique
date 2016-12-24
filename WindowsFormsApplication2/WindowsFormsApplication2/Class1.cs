using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
    public class ImageToSmall : Exception 
    {
        public ImageToSmall()
        {
        }

        public ImageToSmall(string message) :base(message)
        {
        }

        public ImageToSmall(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
