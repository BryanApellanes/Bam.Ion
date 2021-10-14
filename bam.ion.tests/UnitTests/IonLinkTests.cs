using Bam.Net;
using Bam.Net.CommandLine;
using Bam.Net.Testing.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Tests.UnitTests
{
    [Serializable]
    public class IonLinkTests : CommandLineTool
    {
        [UnitTest]
        public void IonLinkShouldIdentifyLinkJson()
        {
            string json = @"{ ""href"": ""https://example.io/corporations/acme"" }";

            Expect.IsTrue(Ion.IsLink(json));
            Expect.IsTrue(IonLink.IsValid(json));
        }

        [UnitTest]
        public void IonLinkShouldSerializeAsExpected()
        {
            IonLink ionLink = new IonLink("employer", "https://example.io/corporations/acme");
            ionLink.AddSupportingMember("name", "Joe");

            string expected = 
            @"{
  ""name"": ""Joe"",
  ""employer"": {
    ""href"": ""https://example.io/corporations/acme""
  }
}";

            string actual = ionLink.ToJson(true);

            Message.PrintDiff(expected, actual);

            Expect.AreEqual(expected, actual);
        }
    }
}
