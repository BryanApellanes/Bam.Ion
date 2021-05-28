using Bam.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class IonLink : IonObject, IIonLink, IJsonable
    {
        WebLink webLink;

        public IonLink() 
        {
            this.webLink = new WebLink();
        }

        public IonLink(string relationType, Iri target)
        {
            this.webLink = new WebLink
            {
                RelationType = relationType,
                Target = target
            };
        }

        [JsonProperty("href")]
        public Iri Href 
        {
            get => webLink.Target;
            set => webLink.Target = value;
        }

        public string ToJson()
        {
            return ToJson(false);
        }

        public string ToJson(bool pretty)
        {
            Dictionary<string, object> toSerialize = new Dictionary<string, object>();

            foreach (string key in SupportingMembers.Keys)
            {
                toSerialize.Add(key, SupportingMembers[key]);
            }

            toSerialize.Add(webLink.RelationType, new { href = Href });

            return toSerialize.ToJson(pretty);
;       }

        public static IonLink Read(string json, string nameKey, string relationTypeKey)
        {
            Dictionary<string, object> parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            IonLink ionLInk = new IonLink(parsed[relationTypeKey].ToString(), parsed["href"].ToString());
            ionLInk.AddSupportingMember("name", parsed[nameKey].ToString());
            return ionLInk;
        }

        public static bool IsValid(string json)
        {
            return IsValid(json, out IonLink ignore);
        }

        public static bool IsValid(string json, out IonLink ionLink)
        {
            ionLink = new IonLink();
            Dictionary<string, object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (keyValuePairs.ContainsKey("href"))
            {
                string url = (string)keyValuePairs["href"];
                if (Iri.IsIri(url, out Iri iri))
                {
                    ionLink.Href = iri;
                    return true;
                }
            }
            return false;
        }
    }
}
