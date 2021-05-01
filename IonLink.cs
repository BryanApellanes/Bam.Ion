using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonLink : WebLink, IIonLink, IJsonable
    {
        public IonLink() { }

        public IonLink(string name, string relationType, Iri target)
        {
            this.Name = name;
            this.RelationType = relationType;
            this.Target = target;
        }

        public string Name { get; set; }

        [JsonProperty("href")]
        public Iri Href 
        {
            get => base.Target;
            set => base.Target = value;
        }

        public string ToJson()
        {
            return ToJson(false);
        }

        public string ToJson(bool pretty)
        {
            return new Dictionary<string, object>
            {
                { "name", Name },
                { RelationType, new { href = Href } }
            }.ToJson(pretty)
;       }

        public static IonLink Read(string json, string nameKey, string relationTypeKey)
        {
            Dictionary<string, object> parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            return new IonLink(parsed[nameKey].ToString(), parsed[relationTypeKey].ToString(), parsed["href"].ToString());
        }
    }
}
