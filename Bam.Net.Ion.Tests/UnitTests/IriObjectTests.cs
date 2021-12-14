using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bam.Net.Ion.Tests.UnitTests
{
    public class IriObjectTests
    {
        [Fact]
        public void IriObjectSerializesAsExpected()
        {
            IriObject iriObject = new IriObject("http://test.cxm");
            string expected = @"{
  ""href"": ""http://test.cxm""
}";
            string actual = iriObject.ToJson(true);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IriObjectCanReadString()
        {
            string jsonIriObject = @"{
  ""href"": ""http://test.cxm""
}";
            IriObject readObject = IriObject.Read(jsonIriObject);

            readObject.Href.ToString().Should().BeEquivalentTo("http://test.cxm/"); // because Iri extends Uri a slash is appended 
        }
    }
}
