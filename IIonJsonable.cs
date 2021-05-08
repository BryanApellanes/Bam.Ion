using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public interface IIonJsonable
    {
        string ToIonJson();
        string ToIonJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore);
    }
}
