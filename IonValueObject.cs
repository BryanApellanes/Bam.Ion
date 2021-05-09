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

namespace Bam.Ion
{
    /// <summary>
    /// Ion value whose value property is of the specified generic type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IonValueObject<T> : IonValueObject
    {
        public static implicit operator IonValueObject<T>(T value)
        {
            return new IonValueObject<T> { Value = value };
        }

        public static implicit operator string(IonValueObject<T> value)
        {
            return value.ToJson();
        }

        public IonValueObject() { }

        public IonValueObject(List<IonMember> members) : base(members)
        {
            this.Value = this.ToInstance();
        }

        public IonValueObject(string json): this(IonMember.ListFromJson(json).ToList())
        {
        }

        public IonValueObject(T value)
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
                    if (!IonValueTypes.All.Contains(typeOfValue))
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
                else if (obj is IonValueObject<T> otherIonObject)
                {
                    return Value.Equals(otherIonObject.Value);
                }
                return Value.Equals(obj);
            }
            return false;
        }
    }

    public class IonValueObject : IonType, IJsonable, IIonJsonable, IEnumerable<IonMember>
    {
        public static implicit operator IonValueObject(string value)
        {
            return new IonValueObject { Value = value };
        }
        
        public static implicit operator string(IonValueObject value)
        {
            return value.ToJson();
        }

        private List<IonMember> _memberList;
        private Dictionary<string, IonMember> _memberDictionary;

        public IonValueObject()
        {
            this._memberList = new List<IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
        }

        public IonValueObject(List<IonMember> members)
        {
            this._memberList = members;
            this.SupportingMembers = new Dictionary<string, object>();
            this.SetMemberDictionary();
        }

        public IonValueObject(List<IonMember> ionMembers, Dictionary<string, object> contextData) : this()
        {
            this._memberList = ionMembers;
            this.SupportingMembers = contextData;
            this.SetMemberDictionary();
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
                if (obj is IonValueObject otherIonObject)
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
            }
        }

        public Dictionary<string, object> SupportingMembers
        {
            get;
            set;
        }

        public IonMember this[string name]
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
                return null;
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
                    if (!IonValueTypes.All.Contains(typeOfValue))
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
        public IonValueObject SetSupportingMember(string name, object data)
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

        public IonValueObject AddSupportingMembers(List<System.Collections.Generic.KeyValuePair<string, object>> keyValuePairs)
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
        public IonValueObject AddSupportingMember(string name, object data = null)
        {
            if (!SupportingMembers.ContainsKey(name))
            {
                SupportingMembers.Add(name, data);
            }

            return this;
        }

        public IonValueObject SetTypeContext<T>()
        {
            return SetTypeContext(typeof(T));
        }

        public IonValueObject SetTypeContext(Type type)
        {
            return SetSupportingMember("type", type.Name);
        }

        public IonValueObject SetTypeFullNameContext<T>()
        {
            return SetTypeFullNameContext(typeof(T));
        }

        public IonValueObject SetTypeFullNameContext(Type type)
        {
            return SetSupportingMember("fullName", type.FullName);
        }

        public IonValueObject SetTypeAssemblyQualifiedNameContext<T>()
        {
            return SetTypeAssemblyQualifiedNameContext(typeof(T));
        }

        public IonValueObject SetTypeAssemblyQualifiedNameContext(Type type)
        {
            return SetSupportingMember("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        public static IonValueObject ReadValue(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            foreach(System.Collections.Generic.KeyValuePair<string, object> keyValuePair in dictionary)
            {
                members.Add(keyValuePair);
            }
            return new IonValueObject(members) { SourceJson = json };
        }

        public static IonValueObject<T> ReadValue<T>(string json)
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

            IonValueObject<T> result = new IonValueObject<T>(members) { SourceJson = json };
            result.SetMemberDictionary();
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

        private void SetMemberDictionary()
        {
            _memberDictionary = new Dictionary<string, IonMember>();
            foreach (IonMember ionMember in _memberList)
            {
                string camelCase = ionMember.Name.CamelCase();
                string pascalCase = ionMember.Name.PascalCase();
                _memberDictionary.Add(camelCase, ionMember);
                _memberDictionary.Add(pascalCase, ionMember);
            }
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
                data = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(Value));
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
