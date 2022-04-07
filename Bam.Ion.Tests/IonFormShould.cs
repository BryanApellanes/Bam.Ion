using FluentAssertions;
using Newtonsoft.Json;
using Bam.Ion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bam.Ion.Tests
{
    public class IonFormShould
    {
        static string testForm = @"{
  ""href"": ""https://example.io/users"", ""rel"":[""create-form""], ""method"": ""POST"",
  ""value"": [
    { ""name"": ""givenName"", ""value"": ""John"" },
    { ""name"": ""surname"", ""value"": ""Smith"" },
    { ""name"": ""username"", ""value"": ""jsmith"" },
    { ""name"": ""password"", ""value"": ""correcthorsebatterystaple"", ""secret"": true },
    { ""name"": ""employer"", ""label"": ""Employer"", ""type"": ""object"", ""form"": {
        ""value"": [
          { ""name"": ""name"", ""label"": ""Name"", ""value"": ""Acme, Inc."" },
          { ""name"": ""foundingYear"", ""label"": ""Founding Year"", ""type"": ""integer"", ""value"": 1900 },
          { ""name"": ""address"", ""label"": ""Employer Postal Address"", ""type"": ""object"", ""form"": {
              ""value"": [
                { ""name"": ""street1"", ""label"": ""Street 1"", ""value"": ""1234 Anywhere Street"" },
                { ""name"": ""street2"", ""label"": ""Street 2"", ""value"": ""Suite 100"" },
                { ""name"": ""city"", ""label"": ""City"", ""value"": ""Anytown"" },
                { ""name"": ""state"", ""label"": ""State"", ""value"": ""NY"" },
                { ""name"": ""zip"", ""label"": ""Zip"", ""value"": ""10001"" }
              ]
            }
          }
        ]
      }
    }
  ]
}";

        [Fact]
        public void BeForm()
        {
            IonValueObject valueObject = IonValueObject.ReadObject(testForm);
            Assert.True(IonForm.IsValid(valueObject, out IonForm? ionForm));
            ionForm.Should().NotBeNull();
            ionForm?.Href.Should().Be("https://example.io/users");
            ionForm?.RelationTypes.Should().NotBeNull();
            ionForm?.RelationTypes?.Contains("create-form").Should().BeTrue();
        }

        [Fact]
        public void CreateFormSubmissionObject()
        {
            string expected = @"{""givenName"":""John"",""surname"":""Smith"",""username"":""jsmith"",""password"":""correcthorsebatterystaple"",""employer"":{""name"":""Acme, Inc."",""foundingYear"":1900,""address"":{""street1"":""1234 Anywhere Street"",""street2"":""Suite 100"",""city"":""Anytown"",""state"":""NY"",""zip"":""10001""}}}";
            string prettyExpected = @"{
  ""givenName"": ""John"",
  ""surname"": ""Smith"",
  ""username"": ""jsmith"",
  ""password"": ""correcthorsebatterystaple"",
  ""employer"": {
    ""name"": ""Acme, Inc."",
    ""foundingYear"": 1900,
    ""address"": {
      ""street1"": ""1234 Anywhere Street"",
      ""street2"": ""Suite 100"",
      ""city"": ""Anytown"",
      ""state"": ""NY"",
      ""zip"": ""10001""
    }
  }
}";
            IonForm form = IonForm.ReadForm(testForm);
            FormSubmissionObject submissionObject = form.GetFormSubmissionObject();
            string json = submissionObject.ToJson();
            string pretty = submissionObject.ToJson(true);
            json.Should().Be(expected);
            pretty.Should().Be(prettyExpected);
        }
    }
}
