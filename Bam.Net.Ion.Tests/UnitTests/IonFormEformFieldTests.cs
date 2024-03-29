﻿using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bam.Net.Ion.Tests.UnitTests
{
    [Serializable]
    public class IonFormEformFieldTests
    {
        /*
 * 

The eform member value is either a Form object or a Link to a Form object that reflects the required object structure of each element in the field’s value array. The name "eform" is short for "element form".
        */

        [Fact]
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
            IonFormField.IsValid(formFieldJson).Should().BeTrue();
        }

        /*If the field’s type member is not equal to array or set, an Ion parser MUST ignore the eform member.*/
        [Fact]
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

            ionFormField["eform"].Should().BeNull();
        }

        [Fact]
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

            ionFormField["eform"].Should().NotBeNull();
        }

        /*  
If the eform member equals null, an Ion parser MUST ignore the eform member.*/
        [Fact]
        public void EformIsIgnoredIfMemberIsNull()
        {
            string formFieldJson = @"{ 
    ""name"": ""profile"",
    ""eform"": null
}";
            IonFormField ionFormField = IonFormField.Read(formFieldJson);

            ionFormField["eform"].Should().BeNull();
        }
        /*
 *     Either:

The JSON object is discovered to be an Ion Link as defined in Section 4 AND its meta member has an internal rel member that contains one of the octet sequences form, edit-form, create-form or query-form, OR:

The JSON object is a member named form inside an Ion Form Field.
 */
        [Fact]
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
            
            formField.Should().BeSameAs(formMember.Parent);
            IonForm.IsValid(formMember.Value, out IonObject ionForm).Should().BeTrue();
            expectedForm.Should().BeEquivalentTo(ionForm.ToJson(true));
        }

        /*
        If the eform member is not a valid Ion Form object, an Ion parser MUST ignore the eform member.*/

        [Fact]
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
            IonFormValidationResult result = IonForm.Validate(formJson);
            result.Should().NotBeNull();
            result.Success.Should().Be(false); // verify that the form is not valid.  form is the same as form in form field
            IonFormField ionFormField = IonFormField.Read(formFieldJson);
            ionFormField["eform"].Should().BeNull(); // should be null because not a valid form (duplicate username fields)
        }
        /* 

If the eform member exists and is valid, and the etype member does not exist or equals null, an Ion parser MUST assign the field an etype member with a value of object.*/

        [Fact]
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
            ionFormField["eform"].Should().NotBeNull();
            ionFormField["etype"].Value.Should().BeEquivalentTo("object");
        }
        /*
        If the etype member does not equal object, an Ion parser MUST ignore the eform member. */
        [Fact]
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
            ionFormField["etype"].Should().BeNull();
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
