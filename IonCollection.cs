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
        public static implicit operator T(IonCollection<T> ionCollection)
        {
            return ionCollection.ToInstance();
        }

        public IonCollection()
        {
            Type = typeof(T);
        }

        public IonCollection(IonCollection other) : this()
        {
            this.Value = other.Value;
            this.ContextData = other.ContextData ?? new Dictionary<string, object>();
        }

        public IonCollection(IonCollection other, Dictionary<string, object> contextData) : this(other)
        {
            foreach(string name in contextData.Keys)
            {
                SetContextData(name, contextData[name]);
            }
        }

        public T ToInstance()
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if(ctor == null)
            {
                throw new InvalidOperationException($"The specified type ({typeof(T).AssemblyQualifiedName}) does not have a parameterless constructor.");
            }
            T instance = (T)ctor.Invoke(null);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach(IonMember ionMember in this)
            {
                ionMember.SetProperty(instance);
            }
            return instance;
        }

        public static IonCollection<T> Read(string json)
        {
            return Read(json, (propertyInfo) => true);
        }

        /// <summary>
        /// Read the specified json using the specified property filter to determine what is a property, with the remaining values added to context data.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="propertyFilter"></param>
        /// <returns></returns>
        public static IonCollection<T> Read(string json, Func<PropertyInfo, bool> propertyFilter)
        {
            IonCollection collection = IonCollection.Read(json);
            HashSet<string> properties = new HashSet<string>();
            foreach (PropertyInfo propertyInfo in typeof(T).GetProperties().Where(propertyFilter))
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
                SetMemberDictionary();
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

        /// <summary>
        /// Move the specified member name to the context data dictionary and remove it from the main member list.
        /// </summary>
        /// <param name="memberName"></param>
        public virtual void SetContextData(string memberName)
        {
            SetContextData(memberName, out _);
        }
        
        /// <summary>
        /// Move the specified member name to the context data dictionary and remove it from the main member list.
        /// </summary>
        /// <param name="memberName"></param>
        /// <param name="member"></param>
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

        /// <summary>
        /// Sets the specified member as context data adding it if necessary, overwriting if a member with the same name already exists.
        /// </summary>
        /// <param name="ionMember">The IonMember.</param>
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

        /// <summary>
        /// Add the specified member if it is not already added.  Throw InvalidOperationException if a member with the same name already exists.
        /// </summary>
        /// <param name="memberName">The name of the member to add.</param>
        /// <param name="value">The value of the member to add.</param>
        public virtual void AddContextData(string memberName, object value)
        {
            AddContextData(new IonMember { Name = memberName, Value = value });
        }

        /// <summary>
        /// Add the specified member if it is not already added.  Throw InvalidOperationException if a member with the same name already exists.
        /// </summary>
        /// <param name="ionMember"></param>
        public virtual void AddContextData(IonMember ionMember)
        {
            if (ContextData.ContainsKey(ionMember.Name))
            {
                throw new InvalidOperationException($"ContextData member with the speciifed name already exists: {ionMember.Name}");
            }
            ContextData.Add(ionMember.Name, ionMember);
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

        [YamlIgnore]
        [JsonIgnore]
        public IonMember this[int index]
        {
            get
            {
                return _innerList[index];
            }
        }

        /// <summary>
        /// Read the specified json as an IonCollection.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
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
                SetMemberDictionary();
            }
        }
        
        private void SetMemberDictionary()
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
