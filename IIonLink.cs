using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public interface IIonLink
    {
        [JsonProperty("href")]
        public Iri Href { get; set; }
    }
}
