using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bam.Net;

namespace Bam.Ion
{
    /// <summary>
    /// A convenience entry point to Ion functionality.
    /// </summary>
    public abstract class Ion
    {
        public IonObjectTypes InferBaseType(string json)
        {
            Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            if (dictionary.ContainsKey("value"))
            {
                object value = dictionary["value"];
                if (value is JObject)
                {
                    return IonObjectTypes.Object;
                }

                if (value is JArray)
                {
                    return IonObjectTypes.Collection;
                }
            }
            return IonObjectTypes.Data;
        }
    }
}
