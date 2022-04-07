using FluentAssertions;
using Bam.Ion;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bam.Ion.Tests
{
    public class IonValueObjectShould
    {
        [Fact]
        public void HaveParentedMembers()
        {
            string json = @"{
  ""value"": ""test value"",
  ""baloney"": ""sandwich""
}";
            IonValueObject valueObject = IonValueObject.ReadObject(json);

            IonMember valueMember = valueObject["value"];
            IonMember baloneyMember = valueObject["baloney"];

            valueMember.Should().NotBeNull();
            baloneyMember.Should().NotBeNull();

            valueObject.Should().BeSameAs(valueMember.Parent);
            valueObject.Should().BeEquivalentTo(valueMember.Parent);
        }

        [Fact]
        public void AddMember()
        {
            IonValueObject value = "test value";
            value.AddMember("baloney", "sandwich");
            string expected = @"{
  ""value"": ""test value"",
  ""baloney"": ""sandwich""
}";
            value.ToJson(true).Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SerializeConvertedString()
        {
            IonValueObject member = "hello";
            string expected = @"{
  ""value"": ""hello""
}";
            string memberJson = member.ToIonJson(true);
            memberJson.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void IonValueObjectShouldSerializeAsExpected()
        {
            string json =
@"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23""
}";

            IonValueObject value = IonValueObject.ReadObject(json);
            string output = value.ToIonJson(true);
            output.Should().BeEquivalentTo(json);
        }

        [Fact]
        public void AddSupportingMembers()
        {
            string expected = "{\r\n  \"value\": \"Hello\",\r\n  \"lang\": \"en\"\r\n}";
            IonValueObject value = "Hello";
            value.SetSupportingMember("lang", "en");
            string output = value.ToIonJson(true);
            output.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SetTypeSupportingMember()
        {
            string json =
                @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23""
}";

            IonValueObject value = IonValueObject.ReadObject(json);
            value.SetTypeContext<TestPerson>();

            string actual = value.ToIonJson(true);
            string expected = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void SetType()
        {
            string json =
                @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23""
}";

            IonValueObject value = IonValueObject.ReadObject(json);
            value.Type = typeof(TestPerson);

            string actual = value.ToIonJson(true);
            string expected = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ParseSupportingMembers()
        {
            string input = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            IonValueObject<TestPerson> testPerson = IonValueObject.ReadObject<TestPerson>(input);
            testPerson.Members.Count.Should().Be(3);
            testPerson.SupportingMembers.Count.Should().Be(1);

            testPerson["firstName"].Value.Should().BeEquivalentTo("Bob");
            testPerson["lastName"].Value.Should().BeEquivalentTo("Smith");
            KeyValuePair<string, object> keyValuePair = testPerson.SupportingMembers.First();
            keyValuePair.Key.Should().BeEquivalentTo("type");
            keyValuePair.Value.Should().BeEquivalentTo("TestPerson");
        }

        [Fact]
        public void ConvertToInstance()
        {
            string input = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            IonValueObject<TestPerson> testPersonIonObject = IonValueObject.ReadObject<TestPerson>(input);
            TestPerson? testPerson = testPersonIonObject.Value;
            testPerson.Should().NotBeNull();
            testPerson.FirstName.Should().BeEquivalentTo("Bob");
            testPerson.LastName.Should().BeEquivalentTo("Smith");
            testPerson.BirthDate.Should().BeEquivalentTo("1980-01-23");
        }

        [Fact]
        public void AddSupportingMember()
        {
            IonValueObject<string> hello = "hello";
            hello.AddSupportingMember("lang", "en");
            string expected = @"{
  ""value"": ""hello"",
  ""lang"": ""en""
}";
            string actual = hello.ToIonJson(true);

            actual.Should().BeEquivalentTo(expected);
        }
    }
}