using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonLinkedForm : IonForm, IIonLink
    {
        [JsonProperty("href")]
        public Iri Href { get; set; }
    }
}
