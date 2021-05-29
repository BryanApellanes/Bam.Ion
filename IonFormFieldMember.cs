using Bam.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Bam.Ion
{
    /// <summary>
    /// Represents a registered form field member.
    /// </summary>
    public abstract class IonFormFieldMember : IonMember
    {
        static HashSet<string> _registeredFormFieldMembers;
        static object _registeredFormFieldMembersLock = new object();
        public static HashSet<string> RegisteredNames
        {
            get
            {
                return _registeredFormFieldMembersLock.DoubleCheckLock(ref _registeredFormFieldMembers, () => new HashSet<string>(new[]
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
                }));
            }
        }

        [YamlIgnore]
        [JsonIgnore]
        public bool Optional { get; protected set; }

        [YamlIgnore]
        [JsonIgnore]
        public string FullName { get; protected set; }

        [YamlIgnore]
        [JsonIgnore]
        public string Description { get; protected set; }

        public virtual bool ParentFieldIsValid(IonObject ionValueObject)
        {
            return true;
        }

        public virtual bool IsValid()
        {
            return true;
        }
                
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

        static object _registeredFormFieldMemberTypesLock = new object();
        static Dictionary<string, Type> _registeredFormFieldMemberTypes;
        public static Dictionary<string, Type> RegisteredFormFieldMemberTypes
        {
            get
            {
                return _registeredFormFieldMemberTypesLock.DoubleCheckLock(ref _registeredFormFieldMemberTypes, () =>
                {
                    Dictionary<string, Type> temp = new Dictionary<string, Type>();
                    Assembly.GetExecutingAssembly()
                       .GetTypes()
                       .Where(type => type.HasCustomAttributeOfType<RegisteredFormFieldMemberAttribute>())
                       .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<RegisteredFormFieldMemberAttribute>() })
                       .Each(val => temp.Add(val.Attribute.MemberName, val.Type));
                    return temp;
                });
            }
        }

        public static bool RegisteredFormFieldIsValid(string registeredMemberName, IonMember member)
        {
            return RegisteredFormFieldIsValid(registeredMemberName, member, out IonFormField ignore);
        }

        public static bool RegisteredFormFieldIsValid(string registeredMemberName, IonMember member, out IonFormField ionFormField)
        {
            ionFormField = ReadRegisteredFormFieldMember(registeredMemberName, member);
            return ionFormField != null;
        }

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

        protected bool JObjectHasMember(string memberName)
        {
            return JObjectHasMember(memberName, out JToken ignore);
        }

        protected bool JObjectHasMember(string memberName, out JToken jToken)
        {
            bool hasMember = JObjectValue.ContainsKey(memberName);
            jToken = JObjectValue[memberName];
            return hasMember;
        }
    }
}
