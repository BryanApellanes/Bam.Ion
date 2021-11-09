using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace Bam.Net.Ion
{
    /// <summary>
    /// A collection of Ion objects.
    /// </summary>
    public class IonCollection : IonObject, IJsonable, IIonJsonable, IEnumerable, IEnumerable<IonObject>
    {
        private List<JToken> _jTokens;
        private List<IonObject> _ionValueObjectList;
        private Dictionary<string, object> _metaData;

        /// <summary>
        /// Instantiate a new IonCollection.
        /// </summary>
        public IonCollection()
        {
            _jTokens = new List<JToken>();
            _ionValueObjectList = new List<IonObject>();
            _metaData = new Dictionary<string, object>();
            Value = _ionValueObjectList;
        }

        /// <summary>
        /// Instantiate a new IonCollection containing the specified values.
        /// </summary>
        /// <param name="ionValues">The values to populate the collection with.</param>
        public IonCollection(List<IonObject> ionValues) 
        {
            _jTokens = new List<JToken>();
            _ionValueObjectList = ionValues;
            _metaData = new Dictionary<string, object>();
            Value = _ionValueObjectList;
        }

        /// <summary>
        /// Instantiate a new IonCollection containing the specified values.
        /// </summary>
        /// <param name="jTokens">The values to populate the collection with.</param>
        public IonCollection(List<JToken> jTokens)
        {
            _jTokens = jTokens;
            _ionValueObjectList = jTokens.Select(jt => new IonObject { Value = jt }).ToList();
            _metaData = new Dictionary<string, object>();
            Value = _ionValueObjectList;
        }

        /// <summary>
        /// Gets the meta data elements.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> MetaDataElements
        {
            get => _metaData;
        }

        /// <summary>
        /// Gets or sets the values in this collection.
        /// </summary>
        public new List<IonObject> Value
        {
            get => _ionValueObjectList;
            set
            {
                _ionValueObjectList = value;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates the values.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        IEnumerator<IonObject> IEnumerable<IonObject>.GetEnumerator()
        {
            return _ionValueObjectList.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates the values.
        /// </summary>
        /// <returns>IEnumerator.</returns>
        public new virtual IEnumerator GetEnumerator()
        {
            return _ionValueObjectList.GetEnumerator();
        }

        /// <summary>
        /// Adds the specified value to the collection.
        /// </summary>
        /// <param name="ionValueObject">The value to add.</param>
        public virtual void Add(IonObject ionValueObject)
        {
            _ionValueObjectList.Add(ionValueObject);
        }
        
        /// <summary>
        /// Adds the specified value to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the specified value.</typeparam>
        /// <param name="json">The json string representation of the value to add.</param>
        public virtual void Add<T>(string json)
        {
            this.Add<T>(new IonObject<T>(json));
        }

        /// <summary>
        /// Adds the specified value to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the specified value.</typeparam>
        /// <param name="ionValueObject">The Ion object to add.</param>
        public virtual void Add<T>(IonObject<T> ionValueObject)
        {
            _ionValueObjectList.Add(ionValueObject);
        }

        /// <summary>
        /// Determines if the collection contains the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>bool</returns>
        public virtual bool Contains(object value)
        {
            return _ionValueObjectList.Contains(value);
        }

        /// <summary>
        /// Gets the count of objects in this collection.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public int Count => _ionValueObjectList.Count;

        /// <summary>
        /// Gets the value at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        [YamlIgnore]
        [JsonIgnore]
        public IonObject this[int index]
        {
            get
            {
                return _ionValueObjectList[index];
            }
        }

        /// <summary>
        /// Returns the json string representation of the collection.
        /// </summary>
        /// <returns></returns>
        public override string ToJson()
        {
            return this.ToJson(false);
        }

        /// <summary>
        /// Returns the json string representation of the collection.
        /// </summary>
        /// <param name="pretty">If true, use indentation.</param>
        /// <param name="nullValueHandling">Specifies null value handling for the underlying JsonSerializer.</param>
        /// <returns>Json string.</returns>
        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return base.ToJson(pretty, nullValueHandling);
        }

        /// <summary>
        /// Returns the Ion json string representation of the collection.
        /// </summary>
        /// <returns>Ion Json string.</returns>
        public override string ToIonJson()
        {
            return ToIonJson(false);
        }

        /// <summary>
        /// Returns the Ion json string representation of the collection.
        /// </summary>
        /// <param name="pretty">If true, use indentation.</param>
        /// <param name="nullValueHandling">Specifies null value handling for the underlying JsonSerializer.</param>
        /// <returns>Json string.</returns>
        public override string ToIonJson(bool pretty, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            List<object> value = new List<object>();
            value.AddRange(_ionValueObjectList.Select(iv => iv.ToDictionary()));
            Dictionary<string, object> toBeSerialized = new Dictionary<string, object>();
            foreach (string key in _metaData.Keys)
            {
                toBeSerialized.Add(key, _metaData[key]);
            }

            toBeSerialized.Add("value", value);
            
            return toBeSerialized.ToJson(pretty, nullValueHandling);
        }

        /// <summary>
        /// Ads the specified meta data.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>The current collection.</returns>
        public IonCollection AddElementMetaData(string name, object value)
        {
            _metaData.Add(name, value);
            return this;
        }

        /// <summary>
        /// Reads the specified json string as an IonCollection.
        /// </summary>
        /// <param name="json">The json string.</param>
        /// <returns>A new IonCollection.</returns>
        public static IonCollection Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<JToken> jTokens = new List<JToken>();
            if (dictionary.ContainsKey("value"))
            {
                JArray arrayValue = dictionary["value"] as JArray;
                foreach(JToken token in arrayValue)
                {
                    jTokens.Add(token);
                }
            }
            IonCollection ionCollection = new IonCollection(jTokens);

            foreach(string key in dictionary.Keys)
            {
                if (!"value".Equals(key))
                {
                    ionCollection.AddElementMetaData(key, dictionary[key]);
                }
            }
            ionCollection.SourceJson = json;
            return ionCollection;
        }

        /// <summary>
        /// Removes the specified object from the collection.
        /// </summary>
        /// <param name="ionObject">The ion object.</param>
        protected void RemoveObject(IonObject ionObject)
        {
            if (_ionValueObjectList.Contains(ionObject))
            {
                _ionValueObjectList.Remove(ionObject);
            }
        }
    }
}
