using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public class LinkRelationType
    {
        public static implicit operator string(LinkRelationType relationType)
        {
            return relationType.ToString();
        }

        public static implicit operator LinkRelationType(string value)
        {
            return new LinkRelationType(value);
        }

        public LinkRelationType() { }

        public LinkRelationType(string value)
        {
            this.Value = value;
        }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}
