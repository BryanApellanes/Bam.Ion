using Bam.Net;
using Bam.Net.Testing.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KeyValuePair = Bam.Net.KeyValuePair;

namespace Bam.Ion.Tests.UnitTests
{
    [Serializable]
    public class IonValueObjectTests : CommandLineTool
    {
        [UnitTest]
        public void MembersAreParented()
        {
            string json = @"{
  ""value"": ""test value"",
  ""baloney"": ""sandwich""
}";
            IonObject valueObject = IonObject.ReadObject(json);

            IonMember valueMember = valueObject["value"];
            Expect.IsNotNull(valueMember);

            IonMember baloneyMember = valueObject["baloney"];
            Expect.IsNotNull(baloneyMember);

            Expect.AreSame(valueObject, valueMember.Parent);
            Expect.AreEqual(valueObject, baloneyMember.Parent);
        }

        [UnitTest]
        public void CanAddMemberToIonObject()
        {
            IonObject value = "test value";
            value.AddMember("baloney", "sandwich");
            string expected = @"{
  ""value"": ""test value"",
  ""baloney"": ""sandwich""
}";
            Expect.AreEqual(expected, value.ToJson(true));
        }

        [UnitTest]
        public void IonValueObjectShouldSerializeConvertedStringAsExpected()
        {
            IonObject member = "hello";
            string expected = @"{
  ""value"": ""hello""
}";
            string memberJson = member.ToIonJson(true);
            Expect.AreEqual(expected, member.ToIonJson(true));
        }

        [UnitTest]
        public void IonValueObjectShouldSerializeAsExpected()
        {
            string json =
@"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23""
}";

            IonObject value = IonObject.ReadObject(json);
            string output = value.ToIonJson(true);
            Expect.AreEqual(json, output);
        }

        [UnitTest]
        public void IonValueObjectCanAddSupportingMembers()
        {
            string expected = "{\r\n  \"value\": \"Hello\",\r\n  \"lang\": \"en\"\r\n}";
            IonObject value = "Hello";
            value.SetSupportingMember("lang", "en");
            string output = value.ToIonJson(true);
            Expect.AreEqual(expected, output);
        }

        [UnitTest]
        public void IonValueObjectCanSetTypeSupportingMember()
        {
            string json =
                @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23""
}";

            IonObject value = IonObject.ReadObject(json);
            value.SetTypeContext<TestPerson>();

            string actual = value.ToIonJson(true);
            string expected = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            Expect.AreEqual(expected, actual);
        }
        
        [UnitTest]
        public void IonValueObjectCanSetType()
        {
            string json =
                @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23""
}";

            IonObject value = IonObject.ReadObject(json);
            value.Type = typeof(TestPerson);

            string actual = value.ToIonJson(true);
            string expected = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            Expect.AreEqual(expected, actual);
        }
        
        [UnitTest]
        public void IonValueObjectParsesSupportingMembers()
        {
            string input = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            IonObject<TestPerson> testPerson = IonObject.ReadObject<TestPerson>(input);
            Expect.AreEqual(3, testPerson.Members.Count, $"Value had {testPerson.Members.Count} items expected 3");
            Expect.AreEqual(1, testPerson.SupportingMembers.Count, $"ContextData had {testPerson.SupportingMembers.Count} items expected 1");
            
            Expect.AreEqual("Bob", testPerson["firstName"].Value);
            Expect.AreEqual("Smith", testPerson["lastName"].Value);
            System.Collections.Generic.KeyValuePair<string, object> keyValuePair = testPerson.SupportingMembers.First();
            Expect.AreEqual("type", keyValuePair.Key);
            Expect.AreEqual("TestPerson", keyValuePair.Value);
        }

        [UnitTest]
        public void IonValueObjectCanConvertToInstance()
        {
            string input = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith"",
  ""birthDate"": ""1980-01-23"",
  ""type"": ""TestPerson""
}";
            IonObject<TestPerson> testPersonIonObject = IonObject.ReadObject<TestPerson>(input);
            TestPerson testPerson = testPersonIonObject.Value;
            Expect.AreEqual("Bob", testPerson.FirstName);
            Expect.AreEqual("Smith", testPerson.LastName);
            Expect.AreEqual("1980-01-23", testPerson.BirthDate);
        }

        [UnitTest]
        public void IonValueObjectCanAddSupportingMember()
        {
            IonObject<string> hello = "hello";
            hello.AddSupportingMember("lang", "en");
            string expected = @"{
  ""value"": ""hello"",
  ""lang"": ""en""
}";
            string actual = hello.ToIonJson(true);

            Expect.AreEqual(expected, actual);
        }
    }
}
