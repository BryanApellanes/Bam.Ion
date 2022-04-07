using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bam.Ion.Tests
{
    [Serializable]
    public class IonLinkShould
    {
        [Fact]
        public void IdentifyLinkJson()
        {
            string json = @"{ ""href"": ""https://example.io/corporations/acme"" }";

            IonApi.IsLink(json).Should().BeTrue();
            IonLink.IsValid(json).Should().BeTrue();
        }

        [Fact]
        public void Serialize()
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
