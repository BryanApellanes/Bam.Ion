using Bam.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Bam.Ion
{
    public abstract class IonType
    {
        public virtual string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return Extensions.ToJson(this, pretty, nullValueHandling);
        }
    }
}
