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
        private List<IonValueObject> _innerList;
        private Dictionary<string, object> _metaData;

        public IonCollection()
        {
            _innerList = new List<IonValueObject>();
            _metaData = new Dictionary<string, object>();
            Value = _innerList;
        }

        public IonCollection(List<IonValueObject> ionValues) 
        {
            _innerList = ionValues;
            _metaData = new Dictionary<string, object>();
            Value = _innerList;
        }

        [JsonIgnore]
        public Dictionary<string, object> MetaDataElements
        {
            get => _metaData;
        }

        public new List<IonValueObject> Value
        {
            get => _innerList;
            set
            {
                _innerList = value;
            }
        }

        IEnumerator<IonValueObject> IEnumerable<IonValueObject>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual void Add(IonValueObject ionValueObject)
        {
            _innerList.Add(ionValueObject);
        }
        
        public virtual void Add<T>(string json)
        {
            this.Add<T>(new IonValueObject<T>(json));
        }

        public virtual void Add<T>(IonValueObject<T> ionValueObject)
        {
            _innerList.Add(ionValueObject);
        }

        public virtual bool Contains(object value)
        {
            return _innerList.Contains(value);
        }

        [YamlIgnore]
        [JsonIgnore]
        public int Count => _innerList.Count;

        [YamlIgnore]
        [JsonIgnore]
        public IonValueObject this[int index]
        {
            get
            {
                return _innerList[index];
            }
        }

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
            Dictionary<string, object> toBeSerialized = new Dictionary<string, object>
            {
                { "value", _innerList }
            };
            return toBeSerialized.ToJson(pretty, nullValueHandling);
        }

        public IonCollection AddElementMetaData(string name, object value)
        {
            _metaData.Add(name, Value);
            return this;
        }

        public static IonCollection Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonValueObject> ionValues = new List<IonValueObject>();
            if (dictionary.ContainsKey("value"))
            {
                JArray arrayValue = dictionary["value"] as JArray;
                foreach(JToken token in arrayValue)
                {
                    ionValues.Add(token.ToJson());
                }
            }
            IonCollection ionCollection = new IonCollection(ionValues);

            foreach(string key in dictionary.Keys)
            {
                if (!"value".Equals(key))
                {
                    ionCollection.AddElementMetaData(key, dictionary[key]);
                }
            }

            return ionCollection;
        }

        protected void RemoveObject(IonValueObject ionObject)
        {
            if (_innerList.Contains(ionObject))
            {
                _innerList.Remove(ionObject);
            }
        }
    }
}
