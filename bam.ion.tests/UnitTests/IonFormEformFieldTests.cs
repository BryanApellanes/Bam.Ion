using Bam.Net;
using Bam.Net.Testing.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Tests.UnitTests
{
    [Serializable]
    public class IonFormEformFieldTests : CommandLineTool
    {
        /*
 * 

The eform member value is either a Form object or a Link to a Form object that reflects the required object structure of each element in the field’s value array. The name "eform" is short for "element form".
        */

        [UnitTest]
        public void EFormIsFormField()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": {
        ""href"": ""https://example.io/profile"",
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
            Expect.IsTrue(IonFormField.IsValid(formFieldJson));
        }

        /*If the field’s type member is not equal to array or set, an Ion parser MUST ignore the eform member.*/
        [UnitTest]
        public void EformIsIgnoredIfNotArrayOrSet()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": {
        ""href"": ""https://example.io/profile"",
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
            IonFormField ionFormField = IonFormField.Read(formFieldJson);

            Expect.IsNull(ionFormField["eform"]);
        }

        [UnitTest]
        public void EformIsNotIgnoredIfArrayOrSet()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": {
        ""rel"": [
          ""form""
        ],
        ""type"": ""array"",
        ""href"": ""https://example.io/profile"",
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
            IonFormField ionFormField = IonFormField.Read(formFieldJson);

            Expect.IsNotNull(ionFormField["eform"]);
        }
        /*  
If the eform member equals null, an Ion parser MUST ignore the eform member.*/
        [UnitTest]
        public void EformIsIgnoredIfMemberIsNull()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": null
}";
            IonFormField ionFormField = IonFormField.Read(formFieldJson);

            Expect.IsNull(ionFormField["eform"]);
        }
        /*
 *     Either:

The JSON object is discovered to be an Ion Link as defined in Section 4 AND its meta member has an internal rel member that contains one of the octet sequences form, edit-form, create-form or query-form, OR:

The JSON object is a member named form inside an Ion Form Field.
 */
        [UnitTest]
        public void TestIonFormFieldMemberIsParentedForm()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""form"": {
  ""type"": ""array"",
  ""href"": ""https://example.io/loginAttempts"",
  ""rel"": [
    ""form""
  ],
  ""method"": ""POST"",
  ""value"": [
    {
      ""name"": ""username""
    },
    {
      ""name"": ""password"",
      ""secret"": true
    }
  ]
}
}";
            string expectedForm = @"{
  ""type"": ""array"",
  ""href"": ""https://example.io/loginAttempts"",
  ""rel"": [
    ""form""
  ],
  ""method"": ""POST"",
  ""value"": [
    {
      ""name"": ""username""
    },
    {
      ""name"": ""password"",
      ""secret"": true
    }
  ]
}";
            IonFormField formField = IonFormField.Read(formFieldJson);

            IonMember formMember = formField["form"];

            Expect.AreSame(formField, formMember.Parent);
            Expect.IsTrue(IonForm.IsValid(formMember.Value, out IonObject ionForm));

            Expect.AreEqual(expectedForm, ionForm.ToJson(true));
        }

        /*
        If the eform member is not a valid Ion Form object, an Ion parser MUST ignore the eform member.*/

        [UnitTest]
        public void EformIsIgnoredIfNotValidForm()
        {
            string formJson = @"{
  ""type"": ""array"",
  ""href"": ""https://example.io/loginAttempts"", ""rel"":[""form""], ""method"": ""POST"",
  ""value"": [
    { ""name"": ""username"" },
    { ""name"": ""username"" },
    { ""name"": ""password"", ""secret"": true }
  ]
}";

            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": {
      ""type"": ""array"",
      ""href"": ""https://example.io/loginAttempts"", ""rel"":[""form""], ""method"": ""POST"",
      ""value"": [
        { ""name"": ""username"" },
        { ""name"": ""username"" },
        { ""name"": ""password"", ""secret"": true }
      ]
    }
}";
            Expect.IsFalse(IonForm.Validate(formJson).Success); // verify that the form is not valid.  form is the same as form in form field
            IonFormField ionFormField = IonFormField.Read(formFieldJson);

            Expect.IsNull(ionFormField["eform"]); // should be null because not a valid form (duplicate username fields)
        }
        /* 

If the eform member exists and is valid, and the etype member does not exist or equals null, an Ion parser MUST assign the field an etype member with a value of object.*/

        [UnitTest]
        public void EtypeShouldBeObjectIfEformIsValid()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": {
        ""rel"": [
          ""form""
        ],
        ""type"": ""array"",
        ""href"": ""https://example.io/profile"",
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
            IonFormField ionFormField = IonFormField.Read(formFieldJson);
            Expect.IsNotNull(ionFormField["eform"]);
            Expect.AreEqual("object", ionFormField["etype"].Value);
        }
        /*
        If the etype member does not equal object, an Ion parser MUST ignore the eform member. */
        [UnitTest]
        public void EtypeShouldBeIgnoredIfNotObject()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""etype"": ""monkey"",
    ""eform"": {
        ""rel"": [
          ""form""
        ],
        ""type"": ""array"",
        ""href"": ""https://example.io/profile"",
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
            IonFormField ionFormField = IonFormField.Read(formFieldJson);
            Expect.IsNull(ionFormField["etype"]);
        }
            /*

    If the eform member is a Link or a Linked Form, Ion parsers MUST NOT submit data to the eform value’s linked href location. 
            The eform’s href location may only be used to read the associated form to determine the structure of the associated form object.*/

   /* If it has been determined that the eform member should be evaluated according to these rules, a validating user agent MUST ensure each element 
    * in the field’s value array conforms to the specified eform form structure before form submission.

    Use of this member is OPTIONAL.

     */
        }
}
