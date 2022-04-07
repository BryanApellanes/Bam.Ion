using FluentAssertions;
using Bam.Ion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bam.Ion.Tests
{
    public class IonFormFieldShould
    {
        [Fact()]
        public void BeForm()
        {
            string json = @"{
    ""name"": ""credentials"",
    ""type"": ""object"",
    ""form"": {
        ""value"": [
            {
                ""name"": ""passcode"",
                ""label"": ""Password"",
                ""secret"": true
            }
        ]
    },
    ""required"": true
}";
            IonFormField formField = new IonFormField(json);
            IonForm form = formField.AsForm();
            form.Should().NotBeNull();
            form.Fields.Count.Should().Be(1);
            IonFormField innerField = form.Fields[0];
            innerField.Name.Should().Be("passcode");
            innerField.Label.Should().Be("Password");
            innerField.Secret.Should().BeTrue();
                
            formField.IsForm(out IonForm outForm).Should().BeTrue();
            outForm.Fields.Count.Should().Be(1);  
            IonFormField outFormField = outForm.Fields[0];
            outFormField.Name.Should().Be("passcode");
            outFormField.Label.Should().Be("Password");
            outFormField.Secret.Should().BeTrue();

        }
    }
}
