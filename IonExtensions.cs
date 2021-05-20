using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bam.Ion
{
    public static class IonExtensions
    {
        public static T ToInstance<T>(this IEnumerable<IonMember> ionMembers)
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (ctor == null)
            {
                throw new InvalidOperationException($"The specified type ({typeof(T).AssemblyQualifiedName}) does not have a parameterless constructor.");
            }
            T instance = (T)ctor.Invoke(null);
            foreach (IonMember ionMember in ionMembers)
            {
                ionMember.SetProperty(instance);
            }
            return instance;
        }

        public static bool IsJson(this string json)
        {
            return IsJson(json, out JObject ignore);
        }

        public static bool IsJson(this string json, out JObject jObject)
        {
            try
            {
                jObject = JObject.Parse(json);
                return true;
            }
            catch (JsonReaderException ex)
            {
                jObject = null;
                return false;
            }
        }
    }
}
