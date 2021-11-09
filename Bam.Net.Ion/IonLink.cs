using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents an Ion link.
    /// </summary>
    public class IonLink : IonObject, IIonLink, IJsonable
    {
        WebLink webLink;

        /// <summary>
        /// Create a new instance of an IonLink.
        /// </summary>
        public IonLink() 
        {
            this.webLink = new WebLink();
        }

        /// <summary>
        /// Create a new instance of an IonLink with the specified relation type and target.
        /// </summary>
        /// <param name="relationType">The relation type.</param>
        /// <param name="target">The target</param>
        public IonLink(string relationType, Iri target)
        {
            this.webLink = new WebLink
            {
                RelationType = relationType,
                Target = target
            };
        }

        /// <summary>
        /// Gets or sets the href.
        /// </summary>
        [JsonProperty("href")]
        public Iri Href
        {
            get => webLink.Target;
            set => webLink.Target = value;
        }

        /// <summary>
        /// Returns the json string representation of the current `IonLink`.
        /// </summary>
        /// <returns>json string.</returns>
        public override string ToJson()
        {
            return ToJson(false);
        }

        /// <summary>
        /// Returns the json string representation of the current `IonLink`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <returns>json string.</returns>
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

        /// <summary>
        /// Read the specified json string as an `IonLink`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="nameKey">The member name to retrieve the name from.</param>
        /// <param name="relationTypeKey">The member name to retrieve the relation type from.</param>
        /// <returns>`IonLink`.</returns>
        public static IonLink Read(string json, string nameKey, string relationTypeKey)
        {
            Dictionary<string, object> parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            IonLink ionLInk = new IonLink(parsed[relationTypeKey].ToString(), parsed["href"].ToString());
            ionLInk.AddSupportingMember("name", parsed[nameKey].ToString());
            return ionLInk;
        }

        /// <summary>
        /// Returns a value indicating if the specified json string is a valid `IonLink`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>`bool`.</returns>
        public static bool IsValid(string json)
        {
            return IsValid(json, out IonLink ignore);
        }

        /// <summary>
        /// <summary>
        /// Returns a value indicating if the specified json string is a valid `IonLink`.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <param name="ionLink">The parsed `IonLink`.</param>
        /// <returns>`bool`.</returns>
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
