using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents an IonObject whose value property is of the specified generic type TValue.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class IonObject<TValue> : IonObject
    {
        public static implicit operator IonObject<TValue>(TValue value)
        {
            return new IonObject<TValue> { Value = value };
        }

        public static implicit operator string(IonObject<TValue> value)
        {
            return value.ToJson();
        }

        /// <summary>
        /// Creates a new instance of `IonObject`.
        /// </summary>
        public IonObject() { }

        /// <summary>
        /// Creates a new instance of `IonObject` whose value is set to the specified members.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonObject(List<IonMember> members) : base(members)
        {
            this.Value = this.ToInstance();
        }

        /// <summary>
        /// Creates a new instance of `IonObject` whose value is set to the members represented by the specified json string.
        /// </summary>
        /// <param name="json">A json string representing the members.</param>
        public IonObject(string json): this(IonMember.ListFromJson(json).ToList())
        {
        }

        /// <summary>
        /// Creates a new instance of `IonObject` with the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public IonObject(TValue value)
        {
            this.Value = value;
        }

        TValue _value;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public new TValue Value
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

        /// <summary>
        /// Returns a json string representing the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the current `IonObject` as an instance of the specified generic type `TValue`.
        /// </summary>
        /// <returns>`TValue`.</returns>
        public TValue ToInstance()
        {
            ConstructorInfo ctor = typeof(TValue).GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new InvalidOperationException($"The specified type ({typeof(TValue).AssemblyQualifiedName}) does not have a parameterless constructor.");
            }
            TValue instance = (TValue)ctor.Invoke(null);
            PropertyInfo[] properties = typeof(TValue).GetProperties();
            foreach (IonMember ionMember in this)
            {
                ionMember.SetProperty(instance);
            }
            return instance;
        }

        /// <summary>
        /// Returns a value uniquely identifying this instance at runtime.
        /// </summary>
        /// <returns>`int`.</returns>
        public override int GetHashCode()
        {
            return this.ToJson(false).GetHashCode();
        }

        /// <summary>
        /// Returns a value indicating if the current instance is equivalent to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>`bool`.</returns>
        public override bool Equals(object value)
        {
            if (value == null && Value == null)
            {
                return true;
            }
            if (value != null && Value == null)
            {
                return false;
            }
            if (value != null && Value != null)
            {
                if (value is string json && json.TryFromJson<TValue>(out TValue otherValue))
                {
                    return Value.ToJson().Equals(otherValue.ToJson());
                }
                else if (value is IonObject<TValue> otherIonObject)
                {
                    return Value.Equals(otherIonObject.Value);
                }
                return Value.Equals(value);
            }
            return false;
        }
    }

    /// <summary>
    /// Represents an `IonObject`.
    /// </summary>
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

        /// <summary>
        /// Creates a new instance of `IonObject`.
        /// </summary>
        public IonObject()
        {
            this._memberList = new List<IonMember>();
            this._memberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a new instance of `IonObject` with the specified members.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonObject(List<IonMember> members)
        {
            this._memberList = members;
            this._memberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
            this.Initialize();
        }

        /// <summary>
        /// Creates a new instance of `IonObject` with the specified members.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonObject(params IonMember[] members) : this(members.ToList())
        {
        }

        /// <summary>
        /// Creates a new instance of `IonObject` with the specified members.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="supportingMembers">The supporting members.</param>
        public IonObject(List<IonMember> ionMembers, Dictionary<string, object> supportingMembers) : this()
        {
            this._memberList = ionMembers;
            this._memberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = supportingMembers;
            this.Initialize();
        }

        static object _registeredMemberLock = new object();
        static HashSet<string> _registeredMembers;
        /// <summary>
        /// Gets registered members as defined by the specification.
        /// </summary>
        public static HashSet<string> RegisteredMembers
        {
            get
            {
                if(_registeredMembers == null)
                {
                    lock (_registeredMemberLock)
                    {
                        if (_registeredMembers == null)
                        {
                            _registeredMembers = new HashSet<string>(new[]
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
                            });
                        }
                    }
                }

                return _registeredMembers;
            }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public string SourceJson { get; internal set; }

        /// <summary>
        /// Returns a representation of the current `IonObject` as a dictionary.
        /// </summary>
        /// <returns>`Dictionary{string, object}`.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach(IonMember ionMember in _memberList)
            {
                dictionary.Add(ionMember.Name, ionMember.Value);
            }
            return dictionary;
        }

        /// <summary>
        /// Returns the current `IonObject` as an instance of generic type `T`.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <returns>{T}.</returns>
        public T ToInstance<T>()
        {
            return IonExtensions.ToInstance<T>(this);
        }

        /// <summary>
        /// Returns a value indicating if the current instance is equivalent to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>`bool`.</returns>
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
                if (obj is IonObject otherIonObject)
                {
                    return Value.Equals(otherIonObject.Value);
                }
                return Value.Equals(obj);
            }
            return false;
        }

        /// <summary>
        /// Returns a value uniquely identifying this instance at runtime.
        /// </summary>
        /// <returns>`int`.</returns>
        public override int GetHashCode()
        {
            if (Value == null)
            {
                return base.GetHashCode();
            }
            return Value.GetHashCode();
        }

        /// <summary>
        /// Gets the members.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the supporting members.
        /// </summary>
        public Dictionary<string, object> SupportingMembers
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the value of the specified member.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>`IonObject`</returns>
        public IonObject ValueOf(string memberName)
        {
            return new IonObject(IonMember.ListFromObject(this[memberName]?.Value).ToArray());
        }

        /// <summary>
        /// Returns the member with the specified name.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>`IonMember`.</returns>
        public IonMember MemberOf(string memberName)
        {
            return this[memberName];
        }

        /// <summary>
        /// Add a member.
        /// </summary>
        /// <param name="name">The name of the member to add.</param>
        /// <param name="value">The value of the member to add.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonObject AddMember(string name, object value)
        {
            return AddMember(new IonMember(name, value));
        }

        /// <summary>
        /// Add the specified member.
        /// </summary>
        /// <param name="ionMember">The member.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonObject AddMember(IonMember ionMember)
        {
            ionMember.Parent = this;
            _memberList.Add(ionMember);
            Initialize();
            return this;
        }

        /// <summary>
        /// Gets or sets a member.
        /// </summary>
        /// <param name="name">The name of the member</param>
        /// <returns>`IonMember`.</returns>
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
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
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
        /// Set the value for the specified supporting member.
        /// </summary>
        /// <param name="name">The name of the supporting member.</param>
        /// <param name="value">The value of the supporting member.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetSupportingMember(string name, object value)
        {
            if(SupportingMembers == null)
            {
                SupportingMembers = new Dictionary<string, object>();
            }

            if (SupportingMembers.ContainsKey(name))
            {
                SupportingMembers[name] = value;
            }
            else
            {
                SupportingMembers.Add(name, value);
            }

            return this;
        }

        /// <summary>
        /// Adds supporting members.
        /// </summary>
        /// <param name="keyValuePairs">The members to add.</param>
        /// <returns>The current `IonObject`.</returns>
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

        /// <summary>
        /// Sets the `type` member to the name of the specified generic type `T`.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetTypeContext<T>()
        {
            return SetTypeContext(typeof(T));
        }

        /// <summary>
        /// Sets the `type` member to the name of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetTypeContext(Type type)
        {
            return SetSupportingMember("type", type.Name);
        }

        /// <summary>
        /// Sets the `fullName` member to the full name of the specified generic type `T`.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetTypeFullNameContext<T>()
        {
            return SetTypeFullNameContext(typeof(T));
        }

        /// <summary>
        /// Sets the `fullName` member to the full name of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetTypeFullNameContext(Type type)
        {
            return SetSupportingMember("fullName", type.FullName);
        }

        /// <summary>
        /// Sets the `assemblyQaulifiedName` member to the assembly qualified name of the specfied generic type `T`.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetTypeAssemblyQualifiedNameContext<T>()
        {
            return SetTypeAssemblyQualifiedNameContext(typeof(T));
        }

        /// <summary>
        /// Sets the `assemblyQaulifiedName` member to the assembly qualified name of the specfied type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonObject SetTypeAssemblyQualifiedNameContext(Type type)
        {
            return SetSupportingMember("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Reads the specified json as an `IonObject`.
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
        
        /// <summary>
        /// Sets the type context.
        /// </summary>
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

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <returns>json string.</returns>
        public virtual string ToJson()
        {
            return ToJson(false);
        }

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns>json string.</returns>
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

        /// <summary>
        /// Returns an Ion json string representation of the current `IonObject`.
        /// </summary>
        /// <returns>Ion json string.</returns>
        public virtual string ToIonJson()
        {
            return ToIonJson(false);
        }

        /// <summary>
        /// Returns an Ion json string representation of the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns>An Ion json string.</returns>
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
