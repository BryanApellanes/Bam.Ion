using Newtonsoft.Json;
using System.Collections.Generic;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents an `IriObject`.
    /// </summary>
    public class IriObject : IJsonable
    {
        /// <summary>
        /// Create a new instance of `IriObject`.
        /// </summary>
        /// <param name="uri">The href value.</param>
        public IriObject(string uri)
        {
            this.Href = new Iri(uri);
        }

        /// <summary>
        /// Creates a new instance of `IriObject`.
        /// </summary>
        /// <param name="iri">The href value.</param>
        public IriObject(Iri iri)
        {
            this.Href = iri;
        }

        /// <summary>
        /// Gets or sets the href.
        /// </summary>
        [JsonProperty("href")]
        public Iri Href { get; set; }

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <returns>json string.</returns>
        public string ToJson(bool pretty)
        {
            return JsonExtensions.ToJson(this, pretty);
        }

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <returns>json string.</returns>
        public string ToJson()
        {
            return this.ToJson(false);
        }

        /// <summary>
        /// Reads the specified json as an `IriObject`.
        /// </summary>
        /// <param name="iriJson"></param>
        /// <returns>`IriObject`.</returns>
        public static IriObject Read(string iriJson)
        {
            Dictionary<string, object> keyValuePairs = JsonConvert.DeserializeObject<Dictionary<string, object>>(iriJson);
            return new IriObject(keyValuePairs["href"].ToString());
        }
    }
}
