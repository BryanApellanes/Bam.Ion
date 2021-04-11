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
            Data = new Dictionary<string, IonMember>();
        }

        public Dictionary<string, IonMember> Data { get; set; }
    }
}
