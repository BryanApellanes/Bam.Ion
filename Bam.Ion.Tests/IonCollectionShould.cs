using Bam.Ion;
using FluentAssertions;
using Xunit;

namespace Bam.Ion.Tests
{
    // TODO: rename class to IonCollectionShould and rename methods accordingly
    public class IonCollectionShould
    {
        [Fact]
        public void SerializeEmpty()
        {
            string expected = @"{
  ""value"": []
}";
            IonCollection collection = new IonCollection();

            string actual = collection.ToIonJson(true);

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ContainValue()
        {
            string testValue = "test Value";

            IonCollection ionCollection = new IonCollection();

            ionCollection.Add(testValue);

            ionCollection.Contains(testValue).Should().BeTrue();
        }

        [Fact]
        public void ShouldContainObjectValue()
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

            ionCollection.Contains(bob).Should().BeTrue();
            ionCollection.Contains(jane).Should().BeTrue();
        }

        [Fact]
        public void ShouldContainIonObjectValue()
        {
            string bob = @"{
  ""firstName"": ""Bob"",
  ""lastName"": ""Smith""
}";
            string jane = @"{
  ""firstName"": ""Jane"",
  ""lastName"": ""Doe""
}";
            IonValueObject<TestPerson> bobObj = new IonValueObject<TestPerson>(bob);
            IonValueObject<TestPerson> janeObj = new IonValueObject<TestPerson>(jane);

            IonCollection ionCollection = new IonCollection();
            ionCollection.Add(bobObj);
            ionCollection.Add(janeObj);

            ionCollection.Contains(bobObj).Should().BeTrue();
            ionCollection.Contains(janeObj).Should().BeTrue();
            ionCollection.Contains(bob).Should().BeTrue();
            ionCollection.Contains(jane).Should().BeTrue();
        }

        [Fact]
        public void HaveElementMetaData()
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
            IonCollection ionCollection = IonCollection.ReadCollection(collectionJson);
            ionCollection.Count.Should().Be(2);
            ionCollection.MetaDataElements.Should().NotBeNull();
            ionCollection.MetaDataElements.Count.Should().Be(1);
        }

        [Fact]
        public void SerializeWithMeta()
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
            IonCollection ionCollection = IonCollection.ReadCollection(collectionJson);
            string json = ionCollection.ToIonJson(true);

            ionCollection.Count.Should().Be(2);
            ionCollection.MetaDataElements.Should().NotBeNull();
            ionCollection.MetaDataElements.Count.Should().Be(1);
            json.Should().BeEquivalentTo(collectionJson);
        }

        [Fact]
        public void SerializeEmptyWithMeta()
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
            IonCollection ionCollection = IonCollection.ReadCollection(sourceJson);
            string json = ionCollection.ToIonJson(true);

            ionCollection.Count.Should().Be(0);
            ionCollection.MetaDataElements.Should().NotBeNull();
            ionCollection.MetaDataElements.Count.Should().Be(1);
            json.Should().BeEquivalentTo(sourceJson);
        }

        [Fact]
        public void DoRoundTrip()
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

            IonCollection ionCollection = IonCollection.ReadCollection(json);
            string output = ionCollection.ToIonJson(true);

            ionCollection.Count.Should().Be(2);
            ionCollection.MetaDataElements.Should().NotBeNull();
            ionCollection.MetaDataElements.Count.Should().Be(9);
            output.Should().BeEquivalentTo(json);
        }
    }
}
