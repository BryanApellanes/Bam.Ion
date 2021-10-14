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
    public class IonFormTests : CommandLineTool
    {
        [UnitTest]
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
            Expect.IsTrue(ionFormValidationResult.Success);
            Expect.AreEqual(expectedFormJson, formMember.Value.ToJson(true));
        }

        [UnitTest]
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
            Expect.IsTrue(isForm);
        }

        [UnitTest]
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
            Expect.IsFalse(isForm);
        }

    }
}
