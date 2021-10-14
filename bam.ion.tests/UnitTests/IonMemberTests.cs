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
    public class IonMemberTests : CommandLineTool
    {
        [UnitTest]
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
            Expect.AreEqual(valueObject, ionMember.Parent);
            Expect.AreSame(valueObject, ionMember.Parent);
            Expect.IsTrue(ionMember.IsMemberNamed("namedChild"));
        }

        [UnitTest]
        public void IonMemberShouldSerializeAsExpected()
        {
            IonMember ionMember = "test";

            string expected = @"{
  ""value"": ""test""
}";

            string actual = ionMember.ToJson(true);

            Expect.AreEqual(expected, actual);
        }

        [UnitTest]
        public void IonMemberShouldToStringAsExpected()
        {
            IonMember ionMember = "test";

            string expected = "\"value\": \"test\"";

            string actual = ionMember.ToString();

            Expect.AreEqual(expected, actual);
        }
    }
}
