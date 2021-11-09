using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bam.Net.Ion.Tests.UnitTests
{
    [Serializable]
    public class IonLinkTests
    {
        [Fact]
        public void IonLinkShouldIdentifyLinkJson()
        {
            string json = @"{ ""href"": ""https://example.io/corporations/acme"" }";

            Ion.IsLink(json).Should().BeTrue();
            IonLink.IsValid(json).Should().BeTrue();
        }

        [Fact]
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

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
