using Bam.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace Bam.Ion
{    
    public class IonCollection : IonValueObject, IJsonable, IIonJsonable, IEnumerable, IEnumerable<IonValueObject>
    {
        private List<JToken> _jTokens;
        private List<IonValueObject> _ionValueObjectList;
        private Dictionary<string, object> _metaData;

        public IonCollection()
        {
            _jTokens = new List<JToken>();
            _ionValueObjectList = new List<IonValueObject>();
            _metaData = new Dictionary<string, object>();
            Value = _ionValueObjectList;
        }

        public IonCollection(List<IonValueObject> ionValues) 
        {
            _jTokens = new List<JToken>();
            _ionValueObjectList = ionValues;
            _metaData = new Dictionary<string, object>();
            Value = _ionValueObjectList;
        }

        public IonCollection(List<JToken> jTokens)
        {
            _jTokens = jTokens;
            _ionValueObjectList = jTokens.Select(jt=> new IonValueObject { Value = jt }).ToList();
            _metaData = new Dictionary<string, object>();
            Value = _ionValueObjectList;
        }

        [JsonIgnore]
        public Dictionary<string, object> MetaDataElements
        {
            get => _metaData;
        }

        public new List<IonValueObject> Value
        {
            get => _ionValueObjectList;
            set
            {
                _ionValueObjectList = value;
            }
        }

        IEnumerator<IonValueObject> IEnumerable<IonValueObject>.GetEnumerator()
        {
            return _ionValueObjectList.GetEnumerator();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _ionValueObjectList.GetEnumerator();
        }

        public virtual void Add(IonValueObject ionValueObject)
        {
            _ionValueObjectList.Add(ionValueObject);
        }
        
        public virtual void Add<T>(string json)
        {
            this.Add<T>(new IonValueObject<T>(json));
        }

        public virtual void Add<T>(IonValueObject<T> ionValueObject)
        {
            _ionValueObjectList.Add(ionValueObject);
        }

        public virtual bool Contains(object value)
        {
            return _ionValueObjectList.Contains(value);
        }

        [YamlIgnore]
        [JsonIgnore]
        public int Count => _ionValueObjectList.Count;

        [YamlIgnore]
        [JsonIgnore]
        public IonValueObject this[int index]
        {
            get
            {
                return _ionValueObjectList[index];
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public string SourceJson { get; set; }

        public override string ToJson()
        {
            return this.ToJson(false);
        }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            return base.ToJson(pretty, nullValueHandling);
        }

        public override string ToIonJson()
        {
            return ToIonJson(false);
        }

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

        public IonCollection AddElementMetaData(string name, object value)
        {
            _metaData.Add(name, value);
            return this;
        }

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

        protected void RemoveObject(IonValueObject ionObject)
        {
            if (_ionValueObjectList.Contains(ionObject))
            {
                _ionValueObjectList.Remove(ionObject);
            }
        }
    }
}
