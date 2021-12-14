using System.Collections.Generic;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents a web link.
    /// </summary>
    public class WebLink
    {
        public WebLink()
        {
            this.TargetAttributes = new List<string>();
        }

        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        public Iri Context { get; set; }

        /// <summary>
        /// Gets or sets the relation type.
        /// </summary>
        public LinkRelationType RelationType { get; set; }

        /// <summary>
        /// Gets or sets the target.
        /// </summary>
        public Iri Target { get; set; }

        /// <summary>
        /// Gets or sets the target attributes.
        /// </summary>
        public List<string> TargetAttributes { get; set; }

        /// <summary>
        /// Returns a string describing this web link.
        /// </summary>
        /// <returns></returns>
        public string Describe()
        {
            string attributeDescription = TargetAttributes?.Count > 0 ? $", which has {string.Join(", ", TargetAttributes)}" : string.Empty;
            return $"{Context?.ToString()} has a {RelationType?.ToString()} resource at {Target?.ToString()}{attributeDescription}";
        }
    }
}
