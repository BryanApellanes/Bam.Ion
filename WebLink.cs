using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class WebLink
    {
        public WebLink()
        {
            this.TargetAttributes = new List<string>();
        }

        public Iri Context { get; set; }

        public LinkRelationType RelationType { get; set; }

        public Iri Target { get; set; }

        public List<string> TargetAttributes { get; set; }

        public string Describe()
        {
            string attributeDescription = TargetAttributes?.Count > 0 ? $", which has {string.Join(", ", TargetAttributes)}" : string.Empty;
            return $"{Context?.ToString()} has a {RelationType?.ToString()} resource at {Target?.ToString()}{attributeDescription}";
        }
    }
}
