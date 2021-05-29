using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net;
using System.Collections;
using System.Reflection;
using YamlDotNet.Serialization;
using Newtonsoft.Json.Linq;

namespace Bam.Ion
{
    /// <summary>
    /// Ion value whose value property is of the specified generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IonObject<T> : IonObject
    {
        public static implicit operator IonObject<T>(T value)
        {
            return new IonObject<T> { Value = value };
        }

        public static implicit operator string(IonObject<T> value)
        {
            return value.ToJson();
        }

        public IonObject() { }

        public IonObject(List<IonMember> members) : base(members)
        {
            this.Value = this.ToInstance();
        }

        public IonObject(string json): this(IonMember.ListFromJson(json).ToList())
        {
        }

        public IonObject(T value)
        {
            this.Value = value;
        }

        T _value;
        public new T Value
        {
            get => _value;
            set
            {
                _value = value;
                base.Value = value;
                if ((this.Members == null || this.Members?.Count == 0) &&
                    _value != null)
                {
                    Type typeOfValue = _value.GetType();
                    if (!IonTypes.All.Contains(typeOfValue))
                    {
                        this.Members = IonMember.ListFromJson(_value?.ToJson()).ToList();
                    }
                }           
            }
        }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (this.Value != null)
            {
                data.Add("value", Value);
            }

            foreach (IonMember member in Members)
            {
                data.Add(member.Name, member.Value);
            }

            foreach (string key in SupportingMembers?.Keys)
            {
                data.Add(key, SupportingMembers[key]);
            }

            return data.ToJson(pretty, nullValueHandling);
        }

        public T ToInstance()
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new InvalidOperationException($"The specified type ({typeof(T).AssemblyQualifiedName}) does not have a parameterless constructor.");
            }
            T instance = (T)ctor.Invoke(null);
            PropertyInfo[] properties = typeof(T).GetProperties();
            foreach (IonMember ionMember in this)
            {
                ionMember.SetProperty(instance);
            }
            return instance;
        }

        public override int GetHashCode()
        {
            return this.ToJson(false).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null && Value == null)
            {
                return true;
            }
            if (obj != null && Value == null)
            {
                return false;
            }
            if (obj != null && Value != null)
            {
                if (obj is string json && json.TryFromJson<T>(out T otherValue))
                {
                    return Value.ToJson().Equals(otherValue.ToJson());
                }
                else if (obj is IonObject<T> otherIonObject)
                {
                    return Value.Equals(otherIonObject.Value);
                }
                return Value.Equals(obj);
            }
            return false;
        }
    }

    public class IonObject : IonType, IJsonable, IIonJsonable, IEnumerable<IonMember>
    {
        public static implicit operator IonObject(string value)
        {
            return new IonObject { Value = value };
        }
        
        public static implicit operator string(IonObject value)
        {
            return value.ToJson();
        }

        private List<IonMember> _memberList;
        private Dictionary<string, IonMember> _memberDictionary;

        public IonObject()
        {
            this._memberList = new List<IonMember>();
            this._memberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
        }

        public IonObject(List<IonMember> members)
        {
            this._memberList = members;
            this._memberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
            this.Initialize();
        }

        public IonObject(params IonMember[] members) : this(members.ToList())
        {
        }

        public IonObject(List<IonMember> ionMembers, Dictionary<string, object> contextData) : this()
        {
            this._memberList = ionMembers;
            this._memberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = contextData;
            this.Initialize();
        }

        static object _registeredMemberLock = new object();
        static HashSet<string> _registeredMembers;
        public static HashSet<string> RegisteredMembers
        {
            get
            {
                return _registeredMemberLock.DoubleCheckLock(ref _registeredMembers, () => new HashSet<string>(new[]
                {
                    "eform",
                    "etype",
                    "form",
                    "href",
                    "method",
                    "accepts",
                    "produces",
                    "rel",
                    "type",
                    "value"
                }));
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public string SourceJson { get; internal set; }

        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach(IonMember ionMember in _memberList)
            {
                dictionary.Add(ionMember.Name, ionMember.Value);
            }
            return dictionary;
        }

        public T ToInstance<T>()
        {
            return IonExtensions.ToInstance<T>(this);
        }

        public override bool Equals(object obj)
        {
            if(obj == null && Value == null)
            {
                return true;
            }
            if(obj != null && Value == null)
            {
                return false;
            }
            if(obj != null && Value != null)
            {
                if (obj is IonObject otherIonObject)
                {
                    return Value.Equals(otherIonObject.Value);
                }
                return Value.Equals(obj);
            }
            return false;
        }

        public List<IonMember> Members
        {
            get
            {
                return _memberList;
            }
            protected set
            {
                _memberList = value;
                Initialize();
            }
        }

        public Dictionary<string, object> SupportingMembers
        {
            get;
            set;
        }

        public IonObject ValueOf(string memberName)
        {
            return new IonObject(IonMember.ListFromObject(this[memberName]?.Value).ToArray());
        }

        public IonMember MemberOf(string memberName)
        {
            return this[memberName];
        }

        public IonObject AddMember(string name, object value)
        {
            return AddMember(new IonMember(name, value));
        }

        public IonObject AddMember(IonMember ionMember)
        {
            ionMember.Parent = this;
            _memberList.Add(ionMember);
            Initialize();
            return this;
        }

        public virtual IonMember this[string name]
        {
            get
            {
                string pascalCase = name.PascalCase();
                if (_memberDictionary.ContainsKey(pascalCase))
                {
                    return _memberDictionary[pascalCase];
                }
                string camelCase = name.CamelCase();
                if (_memberDictionary.ContainsKey(camelCase))
                {
                    return _memberDictionary[camelCase];
                }
                IonMember result = new IonMember { Name = name, Parent = this };
                _memberDictionary.Add(camelCase, result);
                _memberDictionary.Add(pascalCase, result);
                return result;
            }
            set
            {
                if (_memberDictionary.ContainsKey(name))
                {
                    _memberDictionary[name] = value;
                }
                else
                {
                    _memberDictionary.Add(name, value);
                }
            }
        }

        object _value;
        public object Value 
        {
            get => _value;
            set
            {
                _value = value;
                if ((this.Members == null || this.Members?.Count == 0) &&
                   _value != null)
                {
                    Type typeOfValue = _value.GetType();
                    if (!IonTypes.All.Contains(typeOfValue))
                    {
                        this.Members = IonMember.ListFromJson(_value?.ToJson()).ToList();
                    }
                    else if (typeOfValue == typeof(string) && ((string)_value).TryFromJson(out Dictionary<string, object> result))
                    {
                        this.Members = IonMember.ListFromDictionary(result).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// Add supporting member data to this ion value object.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IonObject SetSupportingMember(string name, object data)
        {
            if(SupportingMembers == null)
            {
                SupportingMembers = new Dictionary<string, object>();
            }

            if (SupportingMembers.ContainsKey(name))
            {
                SupportingMembers[name] = data;
            }
            else
            {
                SupportingMembers.Add(name, data);
            }

            return this;
        }

        public IonObject AddSupportingMembers(List<System.Collections.Generic.KeyValuePair<string, object>> keyValuePairs)
        {
            foreach(System.Collections.Generic.KeyValuePair<string, object> kvp in keyValuePairs)
            {
                AddSupportingMember(kvp.Key, kvp.Value);
            }

            return this;
        }

        /// <summary>
        /// Adds the specified supporting member if a supporting member of the same name does
        /// not already exist.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public IonObject AddSupportingMember(string name, object data = null)
        {
            if (!SupportingMembers.ContainsKey(name))
            {
                SupportingMembers.Add(name, data);
            }

            return this;
        }

        public IonObject SetTypeContext<T>()
        {
            return SetTypeContext(typeof(T));
        }

        public IonObject SetTypeContext(Type type)
        {
            return SetSupportingMember("type", type.Name);
        }

        public IonObject SetTypeFullNameContext<T>()
        {
            return SetTypeFullNameContext(typeof(T));
        }

        public IonObject SetTypeFullNameContext(Type type)
        {
            return SetSupportingMember("fullName", type.FullName);
        }

        public IonObject SetTypeAssemblyQualifiedNameContext<T>()
        {
            return SetTypeAssemblyQualifiedNameContext(typeof(T));
        }

        public IonObject SetTypeAssemblyQualifiedNameContext(Type type)
        {
            return SetSupportingMember("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Reads the specified json as an IonValueObject.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IonObject ReadObject(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            foreach(System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                members.Add(keyValuePair);
            }
            return new IonObject(members) { SourceJson = json };
        }

        /// <summary>
        /// Reads the specified json as an IonValueObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IonObject<T> ReadObject<T>(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Dictionary<string, PropertyInfo> properties = IonMember.GetPropertyDictionary(typeof(T));
            List<IonMember> members = new List<IonMember>();
            List<System.Collections.Generic.KeyValuePair<string, object>> supportingMembers = new List<System.Collections.Generic.KeyValuePair<string, object>>();

            foreach (System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                if (properties.ContainsKey(keyValuePair.Key))
                {
                    members.Add(keyValuePair);
                }
                else
                {
                    supportingMembers.Add(keyValuePair);
                }              
            }

            IonObject<T> result = new IonObject<T>(members) { SourceJson = json };
            result.Initialize();
            result.Value = result.ToInstance();
            result.AddSupportingMembers(supportingMembers);
            return result;
        }
        
        protected override void SetTypeContext()
        {
            switch (TypeContextKind)
            {
                case TypeContextKind.Invalid:
                case TypeContextKind.TypeName:
                    SetTypeContext(Type);
                    break;
                case TypeContextKind.FullName:
                    SetTypeFullNameContext(Type);
                    break;
                case TypeContextKind.AssemblyQualifiedName:
                    SetTypeAssemblyQualifiedNameContext(Type);
                    break;
            }
        }

        private void Initialize()
        {
            Dictionary<string, IonMember> temp = new Dictionary<string, IonMember>();
            foreach (IonMember ionMember in _memberList)
            {
                ionMember.Parent = this;
                string camelCase = ionMember.Name.CamelCase();
                string pascalCase = ionMember.Name.PascalCase();
                temp.Add(camelCase, ionMember);
                temp.Add(pascalCase, ionMember);
            }
            _memberDictionary = temp;
        }

        public IEnumerator<IonMember> GetEnumerator()
        {
            return _memberList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _memberList.GetEnumerator();
        }

        public virtual string ToJson()
        {
            return ToJson(false);
        }

        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (this.Value != null)
            {
                if (this.Value is string stringValue)
                {
                    if (stringValue.IsJson(out JObject jObject))
                    {
                        data = jObject.ToObject<Dictionary<string, object>>();
                    }
                    else
                    {
                        data = new Dictionary<string, object>()
                        {
                            {"value", stringValue}
                        };
                    }
                }
                else
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(Value));
                }
            }

            foreach (IonMember member in _memberList)
            {
                data.Add(member.Name, member.Value);
            }

            foreach (string key in SupportingMembers?.Keys)
            {
                data.Add(key, SupportingMembers[key]);
            }
            return data.ToJson(pretty, nullValueHandling);
        }

        public virtual string ToIonJson()
        {
            return ToIonJson(false);
        }

        public virtual string ToIonJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (this.Value != null)
            {
                if(this.Value is IIonJsonable ionJsonable)
                {
                    data.Add("value", ionJsonable.ToIonJson(pretty, nullValueHandling));
                }
                else
                {
                    data.Add("value", Value);
                }
            }

            foreach (IonMember member in _memberList)
            {
                data.Add(member.Name, member.Value);
            }

            foreach (string key in SupportingMembers?.Keys)
            {
                data.Add(key, SupportingMembers[key]);
            }

            return data.ToJson(pretty, nullValueHandling);
        }
    }
}
