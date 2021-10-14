using Bam.Net;
using Bam.Net.Analytics;
using Bam.Net.CommandLine;
using Bam.Net.Testing;
using Bam.Net.Testing.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion.Tests.UnitTests
{
    [Serializable]
    public class IonCollectionTests : CommandLineTool
    {
        [UnitTest]
        public void EmptyIonCollectionSerializesAsExpected()
        {
            string expected = @"{
  ""value"": []
}";
            IonCollection collection = new IonCollection();

            string actual = collection.ToIonJson(true);

            Expect.AreEqual(expected, actual);
        }

        [UnitTest]
        public async Task IonCollectionShouldContainValue()
        {
            string testValue = "test Value";
            When.A<IonCollection>("has a value added to it", ionCollection =>
            {
                ionCollection.Add(testValue);
            })
            .TheTest
            .ShouldPass((because, assertionProvider) =>
            {
                because.ItsTrue("the collection contains the expected value", assertionProvider.Value.Contains(testValue), "the collection did NOT contain the expected value");
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public async Task IonCollectionShouldContainObjectValue()
        {
            string bob = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith""
}";
            string jane = @"{
  ""firstName"": ""Jane"",
  ""lastName"": ""Doe""
}";
            IonCollection ionCollection = new IonCollection();
            ionCollection.Add<TestPerson>(bob);
            ionCollection.Add<TestPerson>(jane);

            Expect.IsTrue(ionCollection.Contains(bob));
            Expect.IsTrue(ionCollection.Contains(jane));
        }

        [UnitTest]
        public async Task IonCollectionShouldConainIonObjectValue()
        {
            string bob = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith""
}";
            string jane = @"{
  ""firstName"": ""Jane"",
  ""lastName"": ""Doe""
}";
            IonObject<TestPerson> bobObj = new IonObject<TestPerson>(bob);
            IonObject<TestPerson> janeObj = new IonObject<TestPerson>(jane);

            IonCollection ionCollection = new IonCollection();
            ionCollection.Add(bobObj);
            ionCollection.Add(janeObj);

            Expect.IsTrue(ionCollection.Contains(bobObj));
            Expect.IsTrue(ionCollection.Contains(janeObj));
            Expect.IsTrue(ionCollection.Contains(bob));
            Expect.IsTrue(ionCollection.Contains(jane));
        }

        [UnitTest]
        public void IonCollectionCanHaveElementMetaData()
        {
           string collectionJson = @"{
    ""eform"": { ""href"": ""https://example.io/users/form"" },
    ""value"": [
        {
        ""firstName"": ""Bob"",
        ""lastName"": ""Smith"",
      },
      {
        ""firstName"": ""Jane"",
        ""lastName"": ""Doe"",
      }
    ]
}";
            IonCollection ionCollection = IonCollection.Read(collectionJson);
            Expect.AreEqual(2, ionCollection.Count);
            Expect.AreEqual(1, ionCollection.MetaDataElements.Count);
        }

        [UnitTest]
        public void IonCollectionWithMetaWillSerializeAsExpected()
        {
            string collectionJson = @"{
  ""eform"": {
    ""href"": ""https://example.io/users/form""
  },
  ""value"": [
    {
      ""firstName"": ""Bob"",
      ""lastName"": ""Smith""
    },
    {
      ""firstName"": ""Jane"",
      ""lastName"": ""Doe""
    }
  ]
}";
            IonCollection ionCollection = IonCollection.Read(collectionJson);
            string json = ionCollection.ToIonJson(true);

            Expect.AreEqual(2, ionCollection.Count);
            Expect.AreEqual(1, ionCollection.MetaDataElements.Count);
            Expect.AreEqual(collectionJson, json);
        }

        [UnitTest]
        public void EmptyIonCollectionWithMetaWillSerializeAsExpected()
        {
            string sourceJson = @"{
  ""self"": {
    ""href"": ""https://example.io/users"",
    ""rel"": [
      ""collection""
    ]
  },
  ""value"": []
}";
            IonCollection ionCollection = IonCollection.Read(sourceJson);

            string json = ionCollection.ToIonJson(true);
            Expect.AreEqual(0, ionCollection.Count);
            Expect.AreEqual(1, ionCollection.MetaDataElements.Count);
            Expect.AreEqual(sourceJson, json);
        }

        [UnitTest]
        public void MoreComplexExampleShouldDoRoundTrip()
        {
            string json = @"{
  ""self"": {
    ""href"": ""https://example.io/users"",
    ""rel"": [
      ""collection""
    ]
  },
  ""desc"": ""Showing 25 of 218 users.  Use the 'next' link for the next page."",
  ""offset"": 0,
  ""limit"": 25,
  ""size"": 218,
  ""first"": {
    ""href"": ""https://example.io/users"",
    ""rel"": [
      ""collection""
    ]
  },
  ""previous"": null,
  ""next"": {
    ""href"": ""https://example.io/users?offset=25"",
    ""rel"": [
      ""collection""
    ]
  },
  ""last"": {
    ""href"": ""https://example.io/users?offset=200"",
    ""rel"": [
      ""collection""
    ]
  },
  ""value"": [
    {
      ""self"": {
        ""href"": ""https://example.io/users/1""
      },
      ""firstName"": ""Bob"",
      ""lastName"": ""Smith"",
      ""birthDate"": ""1977-04-18""
    },
    {
      ""self"": {
        ""href"": ""https://example.io/users/25""
      },
      ""firstName"": ""Jane"",
      ""lastName"": ""Doe"",
      ""birthDate"": ""1980-01-23""
    }
  ]
}";

            IonCollection ionCollection = IonCollection.Read(json);
            string output = ionCollection.ToIonJson(true);
            Expect.AreEqual(2, ionCollection.Count);
            Expect.AreEqual(9, ionCollection.MetaDataElements.Count);
            Expect.AreEqual(json, output);
        }
    }
}
