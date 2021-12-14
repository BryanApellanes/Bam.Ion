using Bam.Net.Ion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Bam.Net.Ion
{
    /// <summary>
    /// Represents a registered form field member.
    /// </summary>
    public abstract class IonFormFieldMember : IonMember
    {
        static HashSet<string> _registeredFormFieldMembers;
        static object _registeredFormFieldMembersLock = new object();

        /// <summary>
        /// Gets the registered form field member names.
        /// </summary>
        public static HashSet<string> RegisteredNames
        {
            get
            {
                if(_registeredFormFieldMembers == null)
                {
                    lock(_registeredFormFieldMembersLock)
                    {
                        if(_registeredFormFieldMembers == null)
                        {
                            _registeredFormFieldMembers = new HashSet<string>(new[]
                            {
                                "desc",
                                "eform",
                                "enabled",
                                "etype",
                                "form",
                                "label",
                                "max",
                                "maxLength",
                                "maxsize",
                                "min",
                                "minLength",
                                "minsize",
                                "mutable",
                                "name",
                                "options",
                                "pattern",
                                "placeHolder",
                                "required",
                                "secret",
                                "type",
                                "value",
                                "visible",
                            });
                        }
                    }
                }

                return _registeredFormFieldMembers;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if this is an optional member.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public bool Optional { get; protected set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public string FullName { get; protected set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [YamlIgnore]
        [JsonIgnore]
        public string Description { get; protected set; }

        /// <summary>
        /// Returns a value indicating if the parent field is valid.  This method should be overridden if parent field validation is necessary, the default value is `true`.
        /// </summary>
        /// <param name="ionParentObject">The parent.</param>
        /// <returns>`bool`.</returns>
        public virtual bool ParentFieldIsValid(IonObject ionParentObject)
        {
            return true;
        }

        /// <summary>
        /// Returns a value indicating if this member is valid.  This method should be overridden if form field member validation is necessary, the default value is `true`.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return true;
        }
                
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public override object Value 
        {
            get => ObjectValue;
            set
            {
                ObjectValue = value;
                Type objectType = ObjectValue.GetType();
                string stringValue = ObjectValue as string;
                if (!IonTypes.All.Contains(objectType) || !string.IsNullOrEmpty(stringValue))
                {
                    if (!string.IsNullOrEmpty(stringValue) && stringValue.IsJson(out JObject jo))
                    {
                        SourceJson = stringValue;
                        JObjectValue = jo;
                        return;
                    }
                    JObject jObject = ObjectValue as JObject;
                    if (jObject != null)
                    {
                        SourceJson = jObject.ToString();
                        JObjectValue = jObject;
                        return;
                    }
                    string objJson = ObjectValue?.ToJson(true) ?? "null";
                    SourceJson = objJson;
                    JObjectValue = JsonConvert.DeserializeObject<JObject>(objJson);
                }
            }
        }

        static Dictionary<string, Func<IonMember, IonFormField>> _registeredFormFieldMemberReaders = new Dictionary<string, Func<IonMember, IonFormField>>()
        {
            { "eform", (member) =>
                {
                    if (RegisteredFormFieldMemberTypes.ContainsKey(member.Name))
                    {
                        if (member.Value == null)
                        {
                            return null;
                        }
                        IonFormFieldMember formField = ContructFormFieldMember(member);
                        if (!formField.IsValid())
                        {
                            return null;
                        }
                    }
                    IonFormField result = new IonFormField(member.Name, IonMember.ListFromObject(member.Value).ToArray());
                    return result;
                }
            }
        };

        private static IonFormFieldMember ContructFormFieldMember(IonMember val)
        {
            return RegisteredFormFieldMemberTypes[val.Name].Construct<IonFormFieldMember>(val.Value);
        }

        static readonly object _registeredFormFieldMemberTypesLock = new object();
        static Dictionary<string, Type> _registeredFormFieldMemberTypes;

        /// <summary>
        /// Gets the registered form field member types.
        /// </summary>
        public static Dictionary<string, Type> RegisteredFormFieldMemberTypes
        {
            get
            {
                if(_registeredFormFieldMemberTypes == null)
                {
                    lock (_registeredFormFieldMemberTypesLock)
                    {
                        if (_registeredFormFieldMemberTypes == null)
                        {
                            Dictionary<string, Type> temp = new Dictionary<string, Type>();
                            Assembly.GetExecutingAssembly()
                               .GetTypes()
                               .Where(type => type.HasCustomAttributeOfType<RegisteredFormFieldMemberAttribute>())
                               .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<RegisteredFormFieldMemberAttribute>() })
                               .Each(val => temp.Add(val.Attribute.MemberName, val.Type));

                            _registeredFormFieldMemberTypes = temp;
                        }
                    }
                }
                return _registeredFormFieldMemberTypes;
            }
        }

        /// <summary>
        /// Returns a value indicating if the member is a valid registered form field.
        /// </summary>
        /// <param name="registeredMemberName">The member name.</param>
        /// <param name="member">The member.</param>
        /// <returns>`bool`.</returns>
        public static bool RegisteredFormFieldIsValid(string registeredMemberName, IonMember member)
        {
            return RegisteredFormFieldIsValid(registeredMemberName, member, out _);
        }

        /// <summary>
        /// Returns a value indicating if the member is a valid registered form field.
        /// </summary>
        /// <param name="registeredMemberName">The member name.</param>
        /// <param name="member">The member.</param>
        /// <param name="ionFormField">The parsed form field.</param>
        /// <returns>`bool`.</returns>
        public static bool RegisteredFormFieldIsValid(string registeredMemberName, IonMember member, out IonFormField ionFormField)
        {
            ionFormField = ReadRegisteredFormFieldMember(registeredMemberName, member);
            return ionFormField != null;
        }

        /// <summary>
        /// Returns a registered form field.
        /// </summary>
        /// <param name="registeredMemberName">The name of the registered member.</param>
        /// <param name="member">The parsed member.</param>
        /// <returns>`IonFormField`.</returns>
        public static IonFormField ReadRegisteredFormFieldMember(string registeredMemberName, IonMember member)
        {
            if (_registeredFormFieldMemberReaders.ContainsKey(registeredMemberName))
            {
                return _registeredFormFieldMemberReaders[registeredMemberName](member);
            }
            if(member?.Value is IJsonable jsonable)
            {
                return IonFormField.Read(jsonable.ToJson());
            }
            if(member?.Value is string stringValue)
            {
                if (stringValue.IsJson())
                {
                    return IonFormField.Read(stringValue);
                }
            }
            stringValue = member?.ToString();
            if (stringValue.IsJson())
            {
                return IonFormField.Read(stringValue);
            }
            return new IonFormField(new IonMember(registeredMemberName, member));
        }

        /// <summary>
        /// Gets or sets the object the Value property was set to.
        /// </summary>
        protected object ObjectValue { get; set; }

        /// <summary>
        /// The resulting JObject from reading the current form field from json input.
        /// </summary>
        protected JObject JObjectValue { get; set; }

        /// <summary>
        /// Gets either the original json parsed into ReadValue or, SetValue serialized if this instance 
        /// was not originally read from json.
        /// </summary>
        protected string SourceJson { get; private set; }

        /// <summary>
        /// Returns a value indicating if the specified member is present.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>`bool`.</returns>
        protected bool JObjectHasMember(string memberName)
        {
            return JObjectHasMember(memberName, out JToken ignore);
        }

        /// <summary>
        /// Returns a value indicating if the specified member is present.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <param name="jToken">The member as a JToken.</param>
        /// <returns>`bool`.</returns>
        protected bool JObjectHasMember(string memberName, out JToken jToken)
        {
            bool hasMember = JObjectValue.ContainsKey(memberName);
            jToken = JObjectValue[memberName];
            return hasMember;
        }
    }
}
