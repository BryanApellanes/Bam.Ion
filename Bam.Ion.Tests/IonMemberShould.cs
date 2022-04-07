using Bam.Ion;
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
    public class IonMemberShould
    {
        [Fact]
        public void BeMemberWithName()
        {
            string json = @"{ 
    ""name"": ""profile"",
    ""namedChild"": {
      ""key"": ""value""
}
}";
            IonValueObject valueObject = IonValueObject.ReadObject(json);
            IonMember ionMember = valueObject["namedChild"];
            valueObject.Should().BeEquivalentTo(ionMember.Parent);
            valueObject.Should().BeSameAs(ionMember.Parent);
            ionMember.IsMemberNamed("namedChild").Should().BeTrue();
        }

        [Fact]
        public void Serialize()
        {
            IonMember ionMember = "test";

            string expected = @"{
  ""value"": ""test""
}";

            string actual = ionMember.ToJson(true);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void OutputExpectedString()
        {
            IonMember ionMember = "test";

            string expected = "\"value\": \"test\"";

            string actual = ionMember.ToString();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
