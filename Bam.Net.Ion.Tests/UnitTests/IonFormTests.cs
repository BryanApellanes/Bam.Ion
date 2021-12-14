using FluentAssertions;
using Bam.Net.Ion;
using Xunit;

namespace Bam.Net.Ion.Tests.UnitTests
{
    public class IonFormTests
    {
        [Fact]
        public void IonFormIsFormIfChildOfFormField()
        {
            string expectedFormJson = @"{
  ""href"": ""https://example.io/profile"",
  ""rel"": [
    ""form""
  ],
  ""value"": [
    {
      ""name"": ""firstName""
    },
    {
      ""name"": ""lastName""
    }
  ]
}";

            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""form"": {
        ""href"": ""https://example.io/profile"",
        ""rel"": [
            ""form""
        ],
        ""value"": [
            {
                ""name"": ""firstName""
            },
            {
                ""name"": ""lastName""
            }
        ]
    }
}";

            IonForm ionForm = IonForm.Read(formFieldJson);
            IonMember formMember = ionForm["form"];
            IonFormValidationResult ionFormValidationResult = IonForm.Validate(formMember);

            ionFormValidationResult.Success.Should().BeTrue();
            formMember.Value.ToJson(true).Should().BeEquivalentTo(expectedFormJson);
        }

        [Fact]
        public void IonFormIsFormTest()
        {
            string formJson = @"

{
  ""href"": ""https://example.io/loginAttempts"", ""rel"":[""form""], ""method"": ""POST"",
  ""value"": [
    { ""name"": ""username"" },
    { ""name"": ""password"", ""secret"": true }
  ]
}";
            bool isForm = Ion.IsForm(formJson);
            isForm.Should().BeTrue();
        }

        [Fact]
        public void IonFormIsNotFormWithDuplicateFieldNamesTest()
        {
            string formJson = @"

{
  ""href"": ""https://example.io/loginAttempts"", ""rel"":[""form""], ""method"": ""POST"",
  ""value"": [
    { ""name"": ""username"" },
    { ""name"": ""username"" },
    { ""name"": ""password"", ""secret"": true }
  ]
}";
            bool isForm = Ion.IsForm(formJson);
            isForm.Should().BeFalse();
        }

    }
}
