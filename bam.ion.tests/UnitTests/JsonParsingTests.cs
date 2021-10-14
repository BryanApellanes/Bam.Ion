using Bam.Net;
using Bam.Net.Testing.Unit;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Tests.UnitTests
{
    [Serializable]
    public class JsonParsingTests: CommandLineTool
    {
        [UnitTest]
        public void JsonArrayParsesAsJArray()
        {
            string arrayJson = @"[
                {
                    ""name"": ""firstName""
                },
                {
                    ""name"": ""lastName""
                }
            ]";

            Expect.IsTrue(arrayJson.IsJsonArray(out JArray jArray));

            Expect.IsInstanceOfType<JArray>(jArray);
        }

        [UnitTest]
        public void ObjectArrayParsesAsJArray()
        {
            object[] array = new[] { new { name = "user" }, new { name = "baloney" } };
            string json = array.ToJson();

            Expect.IsTrue(json.IsJsonArray(out JArray jArray));
            Expect.IsTrue(jArray.Count == 2);
        }
    }
}
