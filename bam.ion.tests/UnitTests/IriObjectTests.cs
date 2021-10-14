using Bam.Net;
using Bam.Net.Testing.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Tests.UnitTests
{
    [Serializable]
    public class IriObjectTests: CommandLineTool
    {
        [UnitTest]
        public void IriObjectSerializesAsExpected()
        {
            IriObject iriObject = new IriObject("http://test.cxm");
            string expected = @"{
  ""href"": ""http://test.cxm""
}";
            string actual = iriObject.ToJson(true);

            Expect.AreEqual(expected, actual);
        }

        [UnitTest]
        public void IriObjectCanReadString()
        {
            string jsonIriObject = @"{
  ""href"": ""http://test.cxm""
}";
            IriObject readObject = IriObject.Read(jsonIriObject);

            Expect.AreEqual("http://test.cxm/", readObject.Href); // because Iri extends Uri a slash is appended 
        }
    }
}
