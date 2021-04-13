using Bam.Net;
using Newtonsoft.Json;
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
    public class IonCollection<T> : IonCollection
    {
        public IonCollection()
        {
            Type = typeof(T);
        }

        public IonCollection(IonCollection other) : this()
        {
            this.Value = other.Value;
        }

        public IonCollection(IonCollection other, Dictionary<string, object> contextData)
        {
            
        }

        public T ToInstance()
        {
            throw new NotImplementedException();
        }

        public static IonCollection<T> Read(string json)
        {
            IonCollection collection = IonCollection.Read(json);
            HashSet<string> properties = new HashSet<string>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties())
            {
                properties.Add(propertyInfo.Name);
                properties.Add(propertyInfo.Name.CamelCase());
            }
            
            IonCollection<T> result = new IonCollection<T>(collection);
            List<IonMember> contextDataToRemove = new List<IonMember>();
            foreach (IonMember ionMember in collection)
            {
                if (!properties.Contains(ionMember.Name))
                {
                    result.SetContextData(ionMember.Name, out IonMember toRemove);
                    contextDataToRemove.Add(toRemove);
                }
            }

            foreach (IonMember ionMember in contextDataToRemove)
            {
                result.RemoveMember(ionMember);
            }

            return result;
        }
    }
    
    public class IonCollection : IonValue, IEnumerable, IEnumerable<IonMember>
    {
        private List<IonMember> _innerList;
        private Dictionary<string, IonMember> _dictionary;

        public IonCollection()
        {
            _innerList = new List<IonMember>();
            Value = _innerList;
        }

        public new List<IonMember> Value
        {
            get => _innerList;
            set
            {
                _innerList = value;
                SetDictionary();
            }
        }

        IEnumerator<IonMember> IEnumerable<IonMember>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        public virtual void Add(object value)
        {
            if (value is IonMember ionMember)
            {
                AddIonMember(ionMember);
            }
            else
            {
                foreach (IonMember member in IonMember.GetMemberList(value))
                {
                    AddIonMember(member);
                }
            }
        }

        public virtual void AddIonMember(IonMember ionMember)
        {
            _innerList.Add(ionMember);
        }
        
        public virtual bool Contains(object value)
        {
            return _innerList.Contains(value);
        }

        public virtual void SetContextData(string memberName)
        {
            SetContextData(memberName, out _);
        }
        
        public virtual void SetContextData(string memberName, out IonMember member)
        {
            member = null;
            IonMember ionMember = _innerList.FirstOrDefault(ionMember => ionMember.Name.Equals(memberName) || ionMember.Name.Equals(memberName.PascalCase()));
            if (ionMember != null)
            {
                SetContextData(ionMember);
                member = ionMember;
            }
        }

        public virtual void SetContextData(IonMember ionMember)
        {
            if (ContextData.ContainsKey(ionMember.Name))
            {
                ContextData[ionMember.Name] = ionMember;
            }
            else
            {
                ContextData.Add(ionMember.Name, ionMember);
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public int Count => _innerList.Count;

        [YamlIgnore]
        [JsonIgnore]
        public IonMember this[string name]
        {
            get
            {
                if (_dictionary.ContainsKey(name))
                {
                    return _dictionary[name];
                }

                string camelCased = name.CamelCase();
                if (_dictionary.ContainsKey(camelCased))
                {
                    return _dictionary[camelCased];
                }

                return null;
            }
        }
        
        public static IonCollection Read(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            foreach(System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                members.Add(keyValuePair);
            }
            return new IonCollection { Value = members };
        }

        public static IonCollection<T> Read<T>(string json)
        {
            return IonCollection<T>.Read(json);
        }
        
        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            foreach(IonMember member in Value)
            {
                data.Add(member.Name, member.Value);
            }
            foreach (string key in ContextData?.Keys)
            {
                data.Add(key, ContextData[key]);
            }
            return data.ToJson(pretty, nullValueHandling);
        }

        protected void RemoveMember(IonMember member)
        {
            if (_innerList.Contains(member))
            {
                _innerList.Remove(member);
                SetDictionary();
            }
        }
        
        private void SetDictionary()
        {
            _dictionary = new Dictionary<string, IonMember>();
            foreach (IonMember ionMember in _innerList)
            {
                _dictionary.Add(ionMember.Name, ionMember);
                _dictionary.Add(ionMember.Name.PascalCase(), ionMember);
            }
        }
    }
}
