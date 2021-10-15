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

        public static bool IsIri(string url, out Iri iri, Action<Exception> exceptionHandler = null)
        {
            iri = null;
            try
            {
                iri = new Iri(url);
                return true;
            }
            catch (Exception ex)
            {
                (exceptionHandler ?? ((exception) => Bam.Net.Logging.Log.Error("Exception parsing iri: {0}", exception, exception.Message)))(ex);
                return false;
            }
        }
    }
}
