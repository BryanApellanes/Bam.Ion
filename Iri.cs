using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    /// <summary>
    /// Represents an Iri, see https://tools.ietf.org/html/rfc3987.  This implementation should be considered a place holder for the extention defined in rfc3987;
    /// for now Iri extends Uri, in the future this will likely not be the case.
    /// </summary>
    public class Iri : Uri
    {
        public static implicit operator string(Iri iri)
        {
            return iri.ToString();
        }
        
        public static implicit operator Iri(string value)
        {
            return new Iri(value);
        }

        public Iri(string uriString) : base(uriString)
        {
        }
    }
}
