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
    public class IonMemberTests
    {
        [Fact]
        public void IsMemberNamedTest()
        {
            string json = @"{ 
    ""name"": ""profile"",
    ""namedChild"": {
      ""key"": ""value""
}
}";
            IonObject valueObject = IonObject.ReadObject(json);
            IonMember ionMember = valueObject["namedChild"];
            valueObject.Should().BeEquivalentTo(ionMember.Parent);
            valueObject.Should().BeSameAs(ionMember.Parent);
            ionMember.IsMemberNamed("namedChild").Should().BeTrue();
        }

        [Fact]
        public void IonMemberShouldSerializeAsExpected()
        {
            IonMember ionMember = "test";

            string expected = @"{
  ""value"": ""test""
}";

            string actual = ionMember.ToJson(true);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IonMemberShouldToStringAsExpected()
        {
            IonMember ionMember = "test";

            string expected = "\"value\": \"test\"";

            string actual = ionMember.ToString();

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
