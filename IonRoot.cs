using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonRoot
    {
        public IonRoot()
        {
            Data = new Dictionary<string, IonObject>();
        }

        public Dictionary<string, IonObject> Data { get; set; }
    }
}
