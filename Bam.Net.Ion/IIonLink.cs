using Newtonsoft.Json;

namespace Bam.Net.Ion
{
    /// <summary>
    /// An Ion Link.
    /// </summary>
    public interface IIonLink
    {
        /// <summary>
        /// Gets or sets the href value.
        /// </summary>
        [JsonProperty("href")]
        Iri Href { get; set; }
    }
}
