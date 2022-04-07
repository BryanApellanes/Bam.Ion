using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Bam.Ion
{
    /// <summary>
    /// Represents an ion value object, see https://ionspec.org/#valueobjects.
    /// </summary>
    public class IonValueObject : IonType, IJsonable, IIonJsonable, IEnumerable<IonMember>
    {
        private static readonly object RegisteredMemberLock = new object();
        private static HashSet<string>? registeredMembers;
        private List<IonMember> memberList;
        private Dictionary<string, IonMember> pascalMemberDictionary;
        private Dictionary<string, IonMember> camelMemberDictionary;
        private object? value;

        public static implicit operator IonValueObject(string value)
        {
            return new IonValueObject { Value = value };
        }

        public static implicit operator string(IonValueObject value)
        {
            return value.ToJson();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject"/> class.
        /// </summary>
        public IonValueObject()
        {
            this.memberList = new List<IonMember>();
            this.pascalMemberDictionary = new Dictionary<string, IonMember>();
            this.camelMemberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonValueObject(List<IonMember> members)
        {
            this.memberList = members;
            this.pascalMemberDictionary = new Dictionary<string, IonMember>();
            this.camelMemberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
            this.Initialize();
        }

        public IonValueObject(IonValueObject ionValue)
        {
            this.memberList = ionValue.Members;
            this.pascalMemberDictionary = new Dictionary<string, IonMember>();
            this.camelMemberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = new Dictionary<string, object>();
            this.Value = ionValue.Value;
            this.Initialize();            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        public IonValueObject(params IonMember[] members)
            : this(members.ToList())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IonValueObject"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="supportingMembers">The supporting members.</param>
        public IonValueObject(List<IonMember> ionMembers, Dictionary<string, object> supportingMembers)
            : this()
        {
            this.memberList = ionMembers;
            this.pascalMemberDictionary = new Dictionary<string, IonMember>();
            this.camelMemberDictionary = new Dictionary<string, IonMember>();
            this.SupportingMembers = supportingMembers;
            this.Initialize();
        }

        private void Initialize()
        {
            Dictionary<string, IonMember> tempCamel = new Dictionary<string, IonMember>();
            Dictionary<string, IonMember> tempPascal = new Dictionary<string, IonMember>();
            foreach (IonMember ionMember in this.memberList)
            {
                ionMember.Parent = this;
                string camelCase = ionMember.Name.CamelCase();
                string pascalCase = ionMember.Name.PascalCase();
                tempCamel.Add(camelCase, ionMember);
                tempPascal.Add(pascalCase, ionMember);
            }

            this.camelMemberDictionary = tempCamel;
            this.pascalMemberDictionary = tempPascal;
        }

        public T? GetMemberValueAs<T>(string memberName) where T: class
        {
            IonMember member = this[memberName];
            return member.Value as T;
        }

        public object GetMemberValue(string memberName)
        {
            return this[memberName].Value;
        }

        public string? GetMemberValueToString(string memberName)
        {
            return this[memberName]?.Value?.ToString();
        }

        public bool? GetMemberBooleanValue(string memberName)
        {
            string? val = GetMemberValueToString(memberName);
            if (val != null)
            {
                return bool.Parse(val);
            }

            return null;
        }

        protected IonValueObject SetValue(string memberName, object value)
        {
            this[memberName] = new IonMember(memberName, value);
            return this;
        }

        /// <summary>
        /// Sets the type context.
        /// </summary>
        protected override void SetTypeContext()
        {
            switch (this.TypeContextKind)
            {
                case TypeContextKind.Invalid:
                case TypeContextKind.TypeName:
                    this.SetTypeContext(this.Type);
                    break;
                case TypeContextKind.FullName:
                    this.SetTypeFullNameContext(this.Type);
                    break;
                case TypeContextKind.AssemblyQualifiedName:
                    this.SetTypeAssemblyQualifiedNameContext(this.Type);
                    break;
            }
        }

        /// <summary>
        /// Gets registered members as defined by the specification.
        /// </summary>
        public static HashSet<string> RegisteredMembers
        {
            get
            {
                if (registeredMembers == null)
                {
                    lock (RegisteredMemberLock)
                    {
                        if (registeredMembers == null)
                        {
                            registeredMembers = new HashSet<string>(new[]
                            {
                                "eform",
                                "etype",
                                "form",
                                "href",
                                "method",
                                "name",
                                "accepts",
                                "produces",
                                "rel",
                                "type",
                                "value",
                            });
                        }
                    }
                }

                return registeredMembers;
            }
        }

        /// <summary>
        /// Gets the source.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public string SourceJson { get; internal set; }

        /// <summary>
        /// Gets the members.
        /// </summary>
        public List<IonMember> Members
        {
            get
            {
                return this.memberList;
            }

            protected set
            {
                this.memberList = value;
                this.Initialize();
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

        public bool HasMember(string memberName)
        {
            return HasMember(memberName, out _);
        }

        public bool HasMember(string memberName, out IonMember ionMember)
        {
            ionMember = this[memberName];
            return ionMember != null;
        }

        public bool HasValidRelArray()
        {
            bool result = true;
            JArray? relArray = this["rel"].Value as JArray;
            if (relArray != null)
            {
                foreach (JToken jToken in relArray)
                {
                    if (!IonForm.FormRelValues.Contains(jToken.ToString()))
                    {
                        result = false;
                        break;
                    }
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public bool HasValueArray()
        {
            return HasValueArray(out _);
        }

        public bool HasValueArray(out JArray arrayValue)
        {
            arrayValue = new JArray();
            if (this["value"]?.Value is JArray valueArray)
            {
                arrayValue = valueArray;
                if (valueArray != null && valueArray.Count > 0)
                {
                    return true;
                }
            }

            return false;
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
                if (this.pascalMemberDictionary.ContainsKey(pascalCase))
                {
                    return this.pascalMemberDictionary[pascalCase];
                }

                string camelCase = name.CamelCase();
                if (this.camelMemberDictionary.ContainsKey(camelCase))
                {
                    return this.camelMemberDictionary[camelCase];
                }

                IonMember result = new IonMember { Name = name, Parent = this };
                this.camelMemberDictionary.Add(camelCase, result);
                this.pascalMemberDictionary.Add(pascalCase, result);
                return result;
            }

            set
            {
                if (char.IsLower(name[0]))
                {
                    if (this.camelMemberDictionary.ContainsKey(name))
                    {
                        this.camelMemberDictionary[name] = value;
                    }
                    else
                    {
                        this.camelMemberDictionary.Add(name, value);
                    }
                }
                else
                {
                    if(this.pascalMemberDictionary.ContainsKey(name))
                    {
                        this.pascalMemberDictionary[name] = value;
                    }
                    else
                    {
                        this.pascalMemberDictionary.Add(name, value);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a representation of the current `IonValueObject` as a dictionary.
        /// </summary>
        /// <returns>`Dictionary{string, object}`.</returns>
        public Dictionary<string, object> ToDictionary()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (IonMember ionMember in this.memberList)
            {
                dictionary.Add(ionMember.Name, ionMember.Value);
            }

            return dictionary;
        }

        public string ValueJson()
        {
            if (this.Value == null)
            {
                return string.Empty;
            }

            if (this.Value is IJsonable jsonable)
            {
                return jsonable.ToJson();
            }

            return JsonConvert.SerializeObject(this.Value);
        }

        /// <summary>
        /// Returns the current IonValueObject as the equivalent IonCollection.
        /// </summary>
        /// <returns></returns>
        public IonCollection Collection()
        {
            if (!string.IsNullOrEmpty(SourceJson))
            {
                return IonCollection.ReadCollection(SourceJson);
            }

            if (this.Value == null)
            {
                return new IonCollection();
            }

            return IonCollection.ReadCollection(ValueJson());
        }

        public List<IonValueObject>? CollectionValue()
        {
            return Collection()?.Value;
        }

        /// <summary>
        /// Returns the current `IonValueObject` as an instance of generic type `T`.
        /// </summary>
        /// <typeparam name="T">The generic type.</typeparam>
        /// <returns>{T}.</returns>
        public T ToInstance<T>()
        {
            return IonExtensions.ToInstance<T>(this["value"].ValueObject());
        }

        /// <summary>
        /// Returns a value indicating if the current instance is equivalent to the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>`bool`.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null && this.Value == null)
            {
                return true;
            }

            if (obj != null && this.Value == null)
            {
                return false;
            }

            if (obj != null && this.Value != null)
            {
                if (obj is IonValueObject otherIonObject)
                {
                    return this.Value.Equals(otherIonObject.Value);
                }

                return this.Value.Equals(obj);
            }

            return false;
        }

        /// <summary>
        /// Returns a value uniquely identifying this instance at runtime.
        /// </summary>
        /// <returns>`int`.</returns>
        public override int GetHashCode()
        {
            if (this.Value == null)
            {
                return base.GetHashCode();
            }

            return this.Value.GetHashCode();
        }

        /// <summary>
        /// Add a member.
        /// </summary>
        /// <param name="name">The name of the member to add.</param>
        /// <param name="value">The value of the member to add.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject AddMember(string name, object value)
        {
            return this.AddMember(new IonMember(name, value));
        }

        /// <summary>
        /// Add the specified member.
        /// </summary>
        /// <param name="ionMember">The member.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject AddMember(IonMember ionMember)
        {
            ionMember.Parent = this;
            this.memberList.Add(ionMember);
            this.Initialize();
            return this;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public virtual object? Value
        {
            get => this.value;
            set
            {
                this.value = value;
                if ((this.Members == null || this.Members?.Count == 0) &&
                   this.value != null)
                {
                    Type typeOfValue = this.value.GetType();
                    if (!IonTypes.All.Contains(typeOfValue))
                    {
                        this.Members = IonMember.ListFromJson(ValueJson()).ToList();
                    }
                    else if (typeOfValue == typeof(string) && ((string)this.value).TryFromJson(out Dictionary<string, object> result))
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
        public IonValueObject SetSupportingMember(string name, object value)
        {
            if (this.SupportingMembers == null)
            {
                this.SupportingMembers = new Dictionary<string, object>();
            }

            if (this.SupportingMembers.ContainsKey(name))
            {
                this.SupportingMembers[name] = value;
            }
            else
            {
                this.SupportingMembers.Add(name, value);
            }

            return this;
        }

        /// <summary>
        /// Adds supporting members.
        /// </summary>
        /// <param name="keyValuePairs">The members to add.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject AddSupportingMembers(List<System.Collections.Generic.KeyValuePair<string, object>> keyValuePairs)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, object> kvp in keyValuePairs)
            {
                this.AddSupportingMember(kvp.Key, kvp.Value);
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
            if (!this.SupportingMembers.ContainsKey(name))
            {
                this.SupportingMembers.Add(name, data);
            }

            return this;
        }

        /// <summary>
        /// Sets the `type` member to the name of the specified generic type `T`.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject SetTypeContext<T>()
        {
            return this.SetTypeContext(typeof(T));
        }

        /// <summary>
        /// Sets the `type` member to the name of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject SetTypeContext(Type type)
        {
            return this.SetSupportingMember("type", type.Name);
        }

        /// <summary>
        /// Sets the `fullName` member to the full name of the specified generic type `T`.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject SetTypeFullNameContext<T>()
        {
            return this.SetTypeFullNameContext(typeof(T));
        }

        /// <summary>
        /// Sets the `fullName` member to the full name of the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject SetTypeFullNameContext(Type type)
        {
            return this.SetSupportingMember("fullName", type.FullName);
        }

        /// <summary>
        /// Sets the `assemblyQaulifiedName` member to the assembly qualified name of the specfied generic type `T`.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject SetTypeAssemblyQualifiedNameContext<T>()
        {
            return this.SetTypeAssemblyQualifiedNameContext(typeof(T));
        }

        /// <summary>
        /// Sets the `assemblyQaulifiedName` member to the assembly qualified name of the specfied type.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <returns>The current `IonObject`.</returns>
        public IonValueObject SetTypeAssemblyQualifiedNameContext(Type type)
        {
            return this.SetSupportingMember("assemblyQualifiedName", type.AssemblyQualifiedName);
        }

        /// <summary>
        /// Reads the specified json as an `IonObject`.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IonValueObject ReadObject(string json)
        {
            Dictionary<string, object>? dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            List<IonMember> members = new List<IonMember>();
            if (dictionary != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in dictionary)
                {
                    members.Add(keyValuePair);
                }
            }

            return new IonValueObject(members) { SourceJson = json };
        }

        /// <summary>
        /// Reads the specified json as an IonObject.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static IonValueObject<T> ReadObject<T>(string json)
        {
            Dictionary<string, object>? dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            Dictionary<string, PropertyInfo> properties = IonMember.GetPropertyDictionary(typeof(T));
            List<IonMember> members = new List<IonMember>();
            List<KeyValuePair<string, object>> supportingMembers = new List<KeyValuePair<string, object>>();

            if (dictionary != null)
            {
                foreach (KeyValuePair<string, object> keyValuePair in dictionary)
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
            }

            IonValueObject<T> result = new IonValueObject<T>(members) { SourceJson = json };
            result.Initialize();
            result.Value = result.ToInstance();
            result.AddSupportingMembers(supportingMembers);
            return result;
        }

        public IEnumerator<IonMember> GetEnumerator()
        {
            return this.memberList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.memberList.GetEnumerator();
        }

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <returns>json string.</returns>
        public virtual string ToJson()
        {
            return this.ToJson(false);
        }

        /// <summary>
        /// Returns a json string representation of the current `IonObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns>json string.</returns>
        public override string ToJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object>? data = new Dictionary<string, object>();
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
                            { "value", stringValue },
                        };
                    }
                }
                else
                {
                    data = JsonConvert.DeserializeObject<Dictionary<string, object>>(ValueJson());
                }
            }

            if (data == null)
            {
                data = new Dictionary<string, object>();
            }

            foreach (IonMember member in this.memberList)
            {
                if(!data.ContainsKey(member.Name))
                {
                    data.Add(member.Name, member.Value);
                }
            }

            foreach (string key in this.SupportingMembers.Keys)
            {
                if (!data.ContainsKey(key))
                {
                    data.Add(key, this.SupportingMembers[key]);
                }
            }

            return data.ToJson(pretty, nullValueHandling);
        }

        /// <summary>
        /// Returns an Ion json string representation of the current `IonObject`.
        /// </summary>
        /// <returns>Ion json string.</returns>
        public virtual string ToIonJson()
        {
            return this.ToIonJson(false);
        }

        /// <summary>
        /// Returns an Ion json string representation of the current `IonValueObject`.
        /// </summary>
        /// <param name="pretty">A value indicating whether to use indentation.</param>
        /// <param name="nullValueHandling">Specifies null handling options for the JsonSerializer.</param>
        /// <returns>An Ion json string.</returns>
        public virtual string ToIonJson(bool pretty = false, NullValueHandling nullValueHandling = NullValueHandling.Ignore)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (this.Value != null)
            {
                if (this.Value is IIonJsonable ionJsonable)
                {
                    data.Add("value", ionJsonable.ToIonJson(pretty, nullValueHandling));
                }
                else
                {
                    data.Add("value", this.Value);
                }
            }

            foreach (IonMember member in this.memberList)
            {
                data.Add(member.Name, member.Value);
            }

            foreach (string key in this.SupportingMembers.Keys)
            {
                data.Add(key, this.SupportingMembers[key]);
            }

            return data.ToJson(pretty, nullValueHandling);
        }

        public virtual bool IsForm()
        {
            return IsForm(out _);
        }

        public virtual bool IsForm(out IonForm? ionForm)
        {
            return IonForm.IsValid(this, out ionForm);
        }

        public virtual IonForm? AsForm()
        {
            return IonForm.ReadForm(this.ToJson());
        }

        public bool IsLink()
        {
            return IsLink(out _);
        }

        public bool IsLink(out IonLink ionLink)
        {
            return IonLink.IsValid(this, out ionLink);
        }
    }
}
